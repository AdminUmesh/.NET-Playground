using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DotNet_Playground.Controllers
{
    public class SerilogController : Controller
    {
        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult LogSuccess()
        {
            Log.Information("✅ Success log created at {Time}", DateTime.Now);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult LogError()
        {
            Log.Error("❌ Error log created at {Time}", DateTime.Now);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult LogInfo()
        {
            Log.Warning("ℹ️ Info log created at {Time}", DateTime.Now);
            return RedirectToAction("Index");
        }
    }
}
