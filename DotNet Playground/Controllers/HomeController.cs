using DotNet_Playground.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace DotNet_Playground.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
    }
}
