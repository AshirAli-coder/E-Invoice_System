using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using System.Linq;

namespace E_Invoice_system.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            var user = _context.users
                .FirstOrDefault(u => u.email == email && u.password == password);

            if (user != null)
            {
                // TEMP session (simple auth)
                HttpContext.Session.SetString("UserEmail", user.email);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(); // 🔥 Redirect nahi, View return karo
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }
    }
}
