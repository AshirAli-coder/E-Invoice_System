using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using System.Linq;

namespace E_Invoice_system.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Invoices";
            // Removed Include(i => i.items) because 'items' does not exist
            var invoices = _context.invoices
                .OrderByDescending(i => i.date)
                .ToList();
            return View(invoices);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Create Invoice";
            ViewBag.Buyers = _context.buyers.Where(b => b.status == "Active").ToList();
            ViewBag.Products = _context.products_services.Where(p => p.status == "Available").ToList();
            ViewBag.Sellers = _context.sellers.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(invoices invoice)
        {
            if (ModelState.IsValid)
            {
                if (invoice.date == default)
                {
                    invoice.date = DateTime.Now;
                }

                if (string.IsNullOrEmpty(invoice.invoice_no))
                {
                    invoice.invoice_no = "INV-" + DateTime.Now.ToString("yyyyMMdd") + "-" + (_context.invoices.Count() + 1).ToString("D3");
                }

                var buyer = _context.buyers.FirstOrDefault(b => b.name == invoice.buyer_name);

                if (buyer != null)
                {
                    invoice.buyer_address = buyer.address;
                    invoice.buyer_contact = buyer.contact;
                }

                _context.invoices.Add(invoice);
                _context.SaveChanges();
                TempData["Success"] = "Invoice generated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Buyers = _context.buyers.Where(b => b.status == "Active").ToList();
            ViewBag.Products = _context.products_services.Where(p => p.status == "Available").ToList();
            ViewBag.Sellers = _context.sellers.ToList();
            return View(invoice);
        }

        public IActionResult Details(int id)
        {
            var invoice = _context.invoices.FirstOrDefault(i => i.id == id);
            if (invoice == null) return NotFound();

            // Fetch expiry date for single-item invoices
            if (!invoice.prod_name_service.Trim().StartsWith("["))
            {
                var product = _context.products_services.FirstOrDefault(p => p.prod_name_service == invoice.prod_name_service);
                ViewBag.SingleItemExpiry = product?.expiry_date?.ToString("yyyy-MM-dd") ?? "N/A";
            }

            ViewData["Title"] = "Invoice #" + (invoice.invoice_no ?? invoice.id.ToString());
            return View(invoice);
        }

        public IActionResult Print(int id)
        {
            var invoice = _context.invoices.FirstOrDefault(i => i.id == id);
            if (invoice == null) return NotFound();

            // Fetch expiry date for single-item invoices
            if (!invoice.prod_name_service.Trim().StartsWith("["))
            {
                var product = _context.products_services.FirstOrDefault(p => p.prod_name_service == invoice.prod_name_service);
                ViewBag.SingleItemExpiry = product?.expiry_date?.ToString("yyyy-MM-dd") ?? "N/A";
            }

            return View(invoice);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var invoice = _context.invoices.Find(id);
            if (invoice != null)
            {
                _context.invoices.Remove(invoice);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public JsonResult GetSalesByBuyer(string buyerName)
        {
            if (string.IsNullOrEmpty(buyerName))
                return Json(new { success = false, message = "Buyer name is required." });

            var sales = _context.sales
                .Where(s => s.buyer_name == buyerName)
                .OrderByDescending(s => s.date)
                .Select(s => new
                {
                    s.id,
                    s.prod_name_service,
                    s.qty_unit_type,
                    s.price,
                    s.discount,
                    s.total_price,
                    s.status,
                    date = s.date.ToString("yyyy-MM-dd"),
                    expiryDate = s.expiry_date.HasValue ? s.expiry_date.Value.ToString("yyyy-MM-dd") : null
                })
                .ToList();

            return Json(new { success = true, sales });
        }
    }
}
