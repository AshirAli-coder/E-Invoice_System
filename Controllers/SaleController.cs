using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace E_Invoice_system.Controllers
{
    public class SaleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SaleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Sales";
            var sales = _context.sales
                .OrderByDescending(s => s.date)
                .ToList();
            return View(sales);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "New Sale";
            ViewBag.Customers = _context.customers.Where(c => c.status == "Active").ToList();
            ViewBag.Products = _context.products_services
                .Where(p => p.status == "Available")
                .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string customer_name, string status, string payment_method, string? description, List<Sale> items)
        {
            // Remove description from validation as it is optional
            ModelState.Remove("description");

            if (items == null || !items.Any())
            {
                ModelState.AddModelError("", "At least one product/service must be added.");
            }

            if (ModelState.IsValid)
            {
                // Ensure description is null if empty
                if (string.IsNullOrWhiteSpace(description)) description = null;

                DateTime now = DateTime.Now;
                foreach (var item in items)
                {
                    item.customer_name = customer_name;
                    item.status = status;
                    item.payment_method = payment_method;
                    item.description = description;
                    item.date = now;

                    // Parse numeric quantity
                    decimal qty = 0;
                    if (!string.IsNullOrEmpty(item.qty_unit_type))
                    {
                        var qtyPart = item.qty_unit_type.Trim().Split(' ')[0];
                        decimal.TryParse(qtyPart, out qty);
                    }

                    // Calculate total
                    item.total_price = (item.price * qty) - item.discount;

                        // Update Inventory
                    var product = _context.products_services.FirstOrDefault(p => p.prod_name_service == item.prod_name_service);
                    if (product != null && !string.IsNullOrEmpty(product.qty_unit_type))
                    {
                        var prodQtyParts = product.qty_unit_type.Split(' ');
                        if (decimal.TryParse(prodQtyParts[0], out decimal currentQty))
                        {
                            string unit = prodQtyParts.Length > 1 ? " " + string.Join(" ", prodQtyParts.Skip(1)) : "";
                            
                            // Check for Stock Limit if selling (qty > 0)
                            if (qty > 0 && currentQty < qty)
                            {
                                ModelState.AddModelError("", $"Insufficient stock for {item.prod_name_service}. Available: {currentQty}, Requested: {qty}");
                                // Since we are inside loop but want to stop everything?
                                // Ef Core tracks changes, so adding error will prevent save if we check ModelState.IsValid again?
                                // No, IsValid is checked at start.
                                // We must throw or return view. 
                                // Proper way: add model error and return view.
                                // But items are already processed partially? No, SaveChanges is at end.
                                
                                // Reload lists
                                ViewBag.Customers = _context.customers.Where(c => c.status == "Active").ToList();
                                ViewBag.Products = _context.products_services.Where(p => p.status == "Available").ToList();
                                return View(); // View expects Model of type Sale, not List<Sale>
                            }

                            currentQty -= qty; // If qty is negative (return), this adds to stock.
                            
                            product.qty_unit_type = $"{currentQty}{unit}";
                            _context.products_services.Update(product);
                        }
                    }
                }
                _context.sales.AddRange(items);
                _context.SaveChanges();
                TempData["Success"] = "Sale created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = _context.customers.Where(c => c.status == "Active").ToList();
            ViewBag.Products = _context.products_services
                .Where(p => p.status == "Available")
                .ToList();
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var sale = _context.sales.FirstOrDefault(s => s.id == id);
                if (sale != null)
                {
                    _context.sales.Remove(sale);
                    _context.SaveChanges();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                // If it's already gone, we don't need to do anything
                if (!_context.sales.Any(s => s.id == id))
                {
                    return RedirectToAction(nameof(Index));
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var sale = _context.sales.FirstOrDefault(s => s.id == id);
            if (sale == null) return NotFound();
            
            ViewData["Title"] = sale.qty_unit_type.Trim().StartsWith("-") ? "Return Details" : "Sale Details";
            return View(sale);
        }

        [HttpGet]
        public JsonResult GetProductDetails(int productId)
        {
            var product = _context.products_services.Find(productId);
            if (product == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                price = product.price,
                discount = product.discount,
                tax = product.tax,
                name = product.prod_name_service
            });
        }
    }
}
