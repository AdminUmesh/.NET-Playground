using DotNet_Playground.Models;
using DotNet_Playground.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using DotNet_Playground.Models;

namespace DotNet_Playground.Controllers
{
    public class EmailController : Controller
    {
        private readonly EmailService _emailService;

        public EmailController()
        {
            _emailService = new EmailService();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(EmailModel model)
        {
            if (ModelState.IsValid)
            {
                await _emailService.SendEmailAsync(model.ToEmail, model.Subject, model.Body);
                ViewBag.Message = "Email sent successfully!";
            }
            return View(model);
        }
    }
}
