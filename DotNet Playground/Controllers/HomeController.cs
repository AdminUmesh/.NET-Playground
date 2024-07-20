using DotNet_Playground.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    }
}
