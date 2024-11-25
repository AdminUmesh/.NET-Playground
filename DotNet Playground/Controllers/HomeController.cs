using DotNet_Playground.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Data;
using System.Net;
using Paytm;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using DotNet_Playground.Helpers;

namespace DotNet_Playground.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult DynamicPage(string Page)
        {
            if (string.IsNullOrEmpty(Page))
            {
                return RedirectToAction("Index", "Home"); // Return an error view if no page is specified
            }

            return View(Page);

        }

        public IActionResult CreateConnectionString(string conn, string mannual, string serv, string db, string id, string pass, string FormChoice, string ForMySql)
        {
            try
            {
                // Read appsettings.json
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var json = System.IO.File.ReadAllText(filePath);
                var jsonObj = JsonConvert.DeserializeObject<JObject>(json);

                // Check if ConnectionStrings section exists
                if (!jsonObj.ContainsKey("ConnectionStrings"))
                {
                    jsonObj["ConnectionStrings"] = new JObject();
                }

                var connectionStrings = jsonObj["ConnectionStrings"] as JObject;

                // Check if the connection name already exists
                if (connectionStrings != null && connectionStrings[conn] != null)
                {
                    //return BadRequest("Connection string with the same name already exists.");
                    TempData["Message"] = "Connection string with the same name already exists.";
                    return View();
                }

                // Update or add the ConnectionStrings section
                if (ForMySql == null && mannual == null)
                {
                    connectionStrings[conn] = $"Server={serv};User ID={id};Password={pass};Database={db};Encrypt=True;TrustServerCertificate=True;";
                }
                else if (mannual != null)
                {
                    connectionStrings[conn] = mannual;
                }
                else
                {
                    connectionStrings[conn] = $"Server={serv}; User ID={id}; Password={pass}; Database={db};";
                }

                // Write updated JSON back to appsettings.json
                string updatedJson = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                System.IO.File.WriteAllText(filePath, updatedJson);

                TempData["Message"] = "Connection string created successfully.";
                return View();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
            return View();
        }

        public IActionResult CreateDb([FromBody] DynamicDatabase request)
        {
            request.DatabaseName = request.ConnectionString; //static
            if (string.IsNullOrEmpty(request.ConnectionString) || string.IsNullOrEmpty(request.DatabaseName) || string.IsNullOrEmpty(request.TableName))
            {
                return BadRequest(new { message = "ConnectionString, DatabaseName, and TableName are required." });
            }

            if (request.DatabaseName == "umesh2")
            {
                try
                {
                    string connString = request.ConnectionString;
                    connString = "Server=localhost;User ID=root;Password=admin@123;";

                    // Create a new database using the connection string
                    using (var connection = new MySqlConnection(connString))
                    {
                        connection.Open();

                        // Check if the database exists
                        string checkDbExistsSql = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @DatabaseName";
                        using (var checkCommand = new MySqlCommand(checkDbExistsSql, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@DatabaseName", request.DatabaseName);
                            var result = checkCommand.ExecuteScalar();

                            // Create database if it doesn't exist
                            if (result == null)
                            {
                                string createDbSql = $"CREATE DATABASE `{request.DatabaseName}`";
                                using (var command = new MySqlCommand(createDbSql, connection))
                                {
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // Now connect to the newly created database to create tables
                    string newConnString = $"{connString};Database={request.DatabaseName}";
                    using (var connection = new MySqlConnection(newConnString))
                    {
                        connection.Open();

                        // Check if the table exists
                        string checkTableExistsSql = @"
                        SELECT COUNT(*)
                        FROM information_schema.tables 
                        WHERE table_schema = DATABASE() 
                        AND table_name = @TableName";
                        using (var checkTableCommand = new MySqlCommand(checkTableExistsSql, connection))
                        {
                            checkTableCommand.Parameters.AddWithValue("@TableName", request.TableName);
                            var tableExists = Convert.ToInt32(checkTableCommand.ExecuteScalar());

                            // Create table if it doesn't exist
                            if (tableExists == 0)
                            {
                                string createTableSql = $@"
                                CREATE TABLE `{request.TableName}` (
                                Id INT AUTO_INCREMENT PRIMARY KEY,
                                Name VARCHAR(100),
                                DOB DATE,
                                Password VARCHAR(100)
                                )";

                                using (var createTableCommand = new MySqlCommand(createTableSql, connection))
                                {
                                    createTableCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    return Ok(new { message = "Database and table created successfully" });
                }
                catch (SqlException sqlEx)
                {
                    
                    return StatusCode(500, new { message = "Database operation failed.", details = sqlEx.Message });
                }
                
            }
            else
            {
                try
                {
                    string connString = request.ConnectionString;
                    connString = "Server=DEV-UMESH\\SQLEXPRESS;User ID=sa;Password=master@123;Database=ecssat;Encrypt=True;TrustServerCertificate=True;";

                    // Create a new database using the connection string
                    using (var connection = new SqlConnection(connString))
                    {
                        connection.Open();

                        // Check if the database exists
                        string checkDbExistsSql = $"SELECT database_id FROM sys.databases WHERE Name = @DatabaseName";
                        using (var checkCommand = new SqlCommand(checkDbExistsSql, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@DatabaseName", request.DatabaseName);
                            var result = checkCommand.ExecuteScalar();

                            // Create database if it doesn't exist
                            if (result == null)
                            {
                                string createDbSql = $"CREATE DATABASE [{request.DatabaseName}]";
                                using (var command = new SqlCommand(createDbSql, connection))
                                {
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // Now connect to the newly created database to create tables
                    string newConnString = $"{connString};Initial Catalog={request.DatabaseName}";
                    using (var connection = new SqlConnection(newConnString))
                    {
                        connection.Open();

                        // Check if the table exists
                        string checkTableExistsSql = $"IF OBJECT_ID(@TableName, 'U') IS NOT NULL SELECT 1 ELSE SELECT 0";
                        using (var checkCommand = new SqlCommand(checkTableExistsSql, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@TableName", request.TableName);
                            var tableExists = (int)checkCommand.ExecuteScalar();

                            // Create table if it doesn't exist
                            if (tableExists == 0)
                            {
                                string createTableSql = $@"
                              CREATE TABLE [{request.TableName}] (
                                 Id INT PRIMARY KEY IDENTITY(1,1),
                                 Name NVARCHAR(100),
                                 DOB DATE,
                                 Password NVARCHAR(100)
                               )";

                                using (var command = new SqlCommand(createTableSql, connection))
                                {
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    return Ok(new { message = "Database and table created successfully" });
                }
                catch (SqlException sqlEx)
                {
                    // Log SQL Exception details for debugging
                    // Log(sqlEx); // Implement your logging mechanism here
                    return StatusCode(500, new { message = "Database operation failed.", details = sqlEx.Message });
                }
                catch (Exception ex)
                {
                    // Log general Exception details for debugging
                    // Log(ex); // Implement your logging mechanism here
                    return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
                }
            }
        }

        public JsonResult SelectConnString()
        {
            // Read appsettings.json
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = System.IO.File.ReadAllText(filePath);
            var jsonObj = JsonConvert.DeserializeObject<JObject>(json);

            // Check if ConnectionStrings section exists
            if (!jsonObj.ContainsKey("ConnectionStrings"))
            {
                TempData["Message"] = "Connection string not found";
                return Json(new { Message = "Connection string not found" });
            }

            var connectionStrings = jsonObj["ConnectionStrings"] as JObject;

            // Convert connection strings to JSON format
            var connectionStringsJson = new JObject();
            foreach (var conn in connectionStrings)
            {
                connectionStringsJson.Add(conn.Key, conn.Value);
            }

            return Json(connectionStringsJson);

        }

        public JsonResult Export_Excel()
        {
            // ADO.NET provider for SQL Server
            //using Microsoft.Data.SqlClient; or //using System.Data.SqlClient; is used for ADO.NET
            List<Std_details> mydata = new List<Std_details>();
            string connectionString = "Server=DEV-UMESH\\SQLEXPRESS;User ID=sa;Password=master@123;Database=YourDB;Encrypt=True;TrustServerCertificate=True;";
            string query = "SELECT * FROM Std_detail";
            // Create a connection object
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create a SqlCommand object
                SqlCommand command = new SqlCommand(query, connection);

                // Create a DataAdapter to fill the DataSet
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                // Create a DataSet to hold the result set
                DataSet dataSet = new DataSet();

                // Fill the DataSet with data from the database
                dataAdapter.Fill(dataSet, "DsName");

                // Now the DataSet contains the result of the query
                // Access the first table in the DataSet
                DataTable dataTable = dataSet.Tables[0];

                // Iterate through the rows of the DataTable
                if (dataSet.Tables["DsName"].Rows.Count > 0)
                {
                    foreach (DataRow dr in dataSet.Tables["DsName"].Rows)
                    {
                        Std_details obj = new Std_details();
                        // Access data by column name (e.g., "ColumnName")
                        obj.id = dr["id"].ToString();
                        obj.name = dr["name"].ToString();
                        obj.roll = dr["roll"].ToString();

                        mydata.Add(obj);
                    }
                }

            }
            return Json(mydata);
        }
        
        [HttpPost]
        public JsonResult Import_Excel(Std_details insdata)
        {
            // Connection string for SQL Server
            string connectionString = "Server=DEV-UMESH\\SQLEXPRESS;User ID=sa;Password=master@123;Database=YourDB;Encrypt=True;TrustServerCertificate=True;";

            // SQL query with parameters
            string query = "INSERT INTO Std_detail (id, name, roll) VALUES (@id, @name, @roll)";

            // Create a connection object
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create a SqlCommand object
                SqlCommand command = new SqlCommand(query, connection);

                // Define parameters and their values
                command.Parameters.AddWithValue("@id", insdata.id);
                command.Parameters.AddWithValue("@name", insdata.name);
                command.Parameters.AddWithValue("@roll", insdata.roll);

                try
                {
                    // Open the connection
                    connection.Open();

                    // Execute the command (insert data)
                    command.ExecuteNonQuery();

                    return Json("Data insert successful");
                }
                catch (Exception ex)
                {
                    // Handle any errors
                    return Json($"Error: {ex.Message}");
                }
            }
        }

        public IActionResult InitiatePayment()
        {

            // Collect payment details (this should come from your database or UI)
            string orderId = "umesh001";
            string txnAmount = "100.00"; // Example amount

            // Create request parameters
            var parameters = new Dictionary<string, string>
            {
                { "MID", _configuration["Paytm:MerchantId"] },
                { "WEBSITE", _configuration["Paytm:Website"] },
                { "CHANNEL_ID", _configuration["Paytm:ChannelId"] },
                { "INDUSTRY_TYPE_ID", _configuration["Paytm:IndustryTypeId"] },
                { "ORDER_ID", orderId },
                { "TXN_AMOUNT", txnAmount },
                { "CUST_ID", "CUSTOMER_001" },
                { "MOBILE_NO", "1234567890" },
                { "EMAIL", "test@example.com" },
                { "CALLBACK_URL", "https://yourwebsite.com/payment/callback" }
            };

            // Generate checksum
            string checksum = PaytmHelper.GenerateChecksum(parameters, _configuration["Paytm:MerchantKey"]);

            // Add checksum to parameters
            parameters.Add("CHECKSUMHASH", checksum);

            // Return a view with these parameters for frontend submission
            ViewBag.PaytmParams = parameters;
            return View();
        }

        [HttpPost]
        public IActionResult Callback()
        {
            // Collect the response from Paytm
            var responseParams = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

            // Verify checksum for the response
            bool isValidChecksum = PaytmHelper.VerifyChecksum(responseParams, responseParams["CHECKSUMHASH"], _configuration["Paytm:MerchantKey"]);

            if (isValidChecksum)
            {
                // Transaction successful
                string status = responseParams["STATUS"];
                string orderId = responseParams["ORDERID"];

                // Handle successful payment (update your database, etc.)
                return View("Success");
            }
            else
            {
                // Checksum verification failed
                return View("Failure");
            }
        }
    }

    
}
