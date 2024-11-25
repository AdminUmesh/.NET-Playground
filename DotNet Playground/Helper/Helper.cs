using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DotNet_Playground.Helpers
{
    public static class PaytmHelper
    {
        // Method to generate checksum hash for payment request
        public static string GenerateChecksum(Dictionary<string, string> parameters, string merchantKey)
        {
            // Sort parameters by keys
            var sortedParams = new SortedDictionary<string, string>(parameters);

            // Concatenate key-value pairs into a query string
            StringBuilder sb = new StringBuilder();
            foreach (var param in sortedParams)
            {
                sb.Append(param.Key).Append("=").Append(param.Value).Append("&");
            }

            // Remove the trailing '&'
            sb.Length = sb.Length - 1;

            // Append the merchant key to the query string
            sb.Append(merchantKey);

            // Generate the checksum hash using SHA256
            string checksum = GenerateHash(sb.ToString());

            return checksum;
        }

        // Method to verify checksum hash for payment response
        public static bool VerifyChecksum(Dictionary<string, string> parameters, string checksumReceived, string merchantKey)
        {
            // Remove the checksum hash from the parameters
            parameters.Remove("CHECKSUMHASH");

            // Generate checksum from the parameters
            string checksumGenerated = GenerateChecksum(parameters, merchantKey);

            // Compare the generated checksum with the received checksum
            return checksumGenerated.Equals(checksumReceived, StringComparison.OrdinalIgnoreCase);
        }

        // Helper method to generate SHA256 hash
        private static string GenerateHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
