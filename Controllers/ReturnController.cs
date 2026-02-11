using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using System.Linq;

namespace E_Invoice_system.Controllers
{
    public class ReturnController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReturnController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Customer Returns";
            var returns = _context.returns
                .OrderByDescending(r => r.ReturnDate)
                .ToList();
            return View(returns);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var returnRecord = _context.returns.Find(id);
            if (returnRecord != null)
            {
                _context.returns.Remove(returnRecord);
                _context.SaveChanges();
                TempData["Success"] = "Return record deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
