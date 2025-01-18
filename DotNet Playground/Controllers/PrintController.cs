using Microsoft.AspNetCore.Mvc;
using System;
using System.Runtime.InteropServices;

namespace RestaurantBillPrinter.Controllers
{
    public class PrintController : Controller
    {
        
        public IActionResult PrintBill()
        {
            string printerName = "POS-80"; // Replace with your printer's name

            // Create the bill content
            string rawData = GenerateBill();

            // Add auto-cutter command
            rawData += "\x1B\x69"; // Full cut command (ESC i)

            try
            {
                bool success = RawPrinterHelper.SendStringToPrinter(printerName, rawData);
                if (success)
                {
                    return Ok("Bill printed successfully!");
                }
                else
                {
                    return StatusCode(500, "Failed to print the bill.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private string GenerateBill()
        {
            // ESC/POS commands for alignment and font
            string esc = "\x1B"; // Escape character
            string newLine = "\n";
            string centerAlign = esc + "a" + "1"; // Center alignment command
            string leftAlign = esc + "a" + "0"; // Left alignment command
            string rightAlign = esc + "a" + "2"; // Left alignment command
            string doubleFont = esc + "!" + "\x38"; // Double-width and double-height font
            string normalFont = esc + "!" + "\x00"; // Normal font

            // Indivisual features
            string Bold = esc + "!" + "\x08";
            string Double_Width = esc + "!" + "\x20";
            string Double_Height = esc + "!" + "\x10";

            // Combined Features
            string Bold_DoubleWidth = esc + "!" + "\x28";
            string Bold_DoubleHeight = esc + "!" + "\x18";
            string DoubleWidth_DoubleHeight = esc + "!" + "\x30";
            string Bold_DoubleWidth_DoubleHeight = esc + "!" + "\x38";

            // Define header with centered text and double font size
            string header =
                centerAlign + doubleFont +
                "RESTAURANT NAME" + newLine +
                "Delicious Food & More" + newLine +
                normalFont +
                "--------------------------------" + newLine;

            // Define items in left-aligned format
            string items =
                leftAlign +
                "Item               Qty    Price" + newLine +
                "--------------------------------" + newLine +
                "Burger             1     150.00" + newLine +
                "Pizza              2     400.00" + newLine +
                "Cold Drink         3     120.00" + newLine +
                "--------------------------------" + newLine;

            // Subtotal, GST, and total calculations
            double total = 670.00; // Subtotal
            double gst = total * 0.18; // 18% GST
            double grandTotal = total + gst;

            // Footer with centered thank-you message
            string footer =
                leftAlign +
                $"Subtotal:              {total:F2}" + newLine +
                $"GST (18%):             {gst:F2}" + newLine +
                $"Grand Total:           {grandTotal:F2}" + newLine +
                "--------------------------------" + newLine;

            // Add extra space between footer and Thank You
            string spacing = new string('\n', 3);  // Add blank lines for spacing

            // Thank You message with centered text and double font size
            string Thanku =
            centerAlign + doubleFont +
            "THANK YOU, VISIT AGAIN!" + newLine +
            normalFont +
            "--------------------------------" + newLine +
            newLine + newLine; // Add extra blank lines

            // Combine all parts
            string all = header + items + footer + Thanku;

            // Add buffer flush and cut commands
            all += "\x1B\x64\x03"; // Buffer flush and cut and 3 line space
           // all += "\x1B\x69";  only page cut cut

            return all;
        }
    }

    public class RawPrinterHelper
    {
        [DllImport("winspool.drv", CharSet = CharSet.Unicode)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref DOCINFO pDocInfo);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool SendStringToPrinter(string printerName, string data)
        {
            IntPtr hPrinter;
            DOCINFO docInfo = new DOCINFO
            {
                pDocName = "Restaurant Bill",
                pDataType = "RAW"
            };

            if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
            {
                throw new ApplicationException("Cannot open printer.");
            }

            try
            {
                if (!StartDocPrinter(hPrinter, 1, ref docInfo))
                {
                    throw new ApplicationException("Cannot start document on printer.");
                }

                if (!StartPagePrinter(hPrinter))
                {
                    throw new ApplicationException("Cannot start page on printer.");
                }

                IntPtr pData = Marshal.StringToHGlobalAnsi(data);
                int dwWritten;
                WritePrinter(hPrinter, pData, data.Length, out dwWritten);
                Marshal.FreeHGlobal(pData);

                EndPagePrinter(hPrinter);
                EndDocPrinter(hPrinter);

                return true;
            }
            finally
            {
                ClosePrinter(hPrinter);
            }
        }

        public struct DOCINFO
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pDataType;
        }
    }
}
