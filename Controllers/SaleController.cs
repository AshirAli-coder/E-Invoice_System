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
            
            var returns = _context.returns
                .OrderByDescending(r => r.ReturnDate)
                .ToList();
            
            ViewBag.Returns = returns;

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
                var salesToInsert = new List<Sale>();
                var returnsToInsert = new List<ReturnDetail>();

                foreach (var item in items)
                {
                    item.customer_name = customer_name;
                    item.status = status;
                    item.payment_method = payment_method;
                    item.description = description;
                    item.date = now;

                    // Parse numeric quantity
                    decimal qty = 0;
                    string unit = "";
                    if (!string.IsNullOrEmpty(item.qty_unit_type))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(item.qty_unit_type.Trim(), @"^([0-9.-]+)\s*(.*)$");
                        if (match.Success)
                        {
                            decimal.TryParse(match.Groups[1].Value, out qty);
                            unit = match.Groups[2].Value;
                        }
                    }

                    // Calculate total
                    item.total_price = (item.price * qty) - item.discount;

                    // PROCESS AS RETURN
                    if (qty < 0)
                    {
                        var originalSale = _context.sales
                            .Where(s => s.customer_name == customer_name && s.prod_name_service == item.prod_name_service)
                            .OrderByDescending(s => s.date)
                            .FirstOrDefault();

                        if (originalSale != null)
                        {
                            // Parse original quantity and UNIT
                            decimal originalQty = 0;
                            string originalUnit = "";
                            var originalMatch = System.Text.RegularExpressions.Regex.Match(originalSale.qty_unit_type ?? "", @"^([0-9.-]+)\s*(.*)$");
                            if (originalMatch.Success) 
                            {
                                decimal.TryParse(originalMatch.Groups[1].Value, out originalQty);
                                originalUnit = originalMatch.Groups[2].Value;
                            }

                            // Update original sale: quantity and price
                            decimal newQty = originalQty + qty; // qty is negative
                            if (newQty < 0) newQty = 0;
                            
                            // Use originalUnit to preserve the unit type (e.g. "Pcs", "Kg")
                            originalSale.qty_unit_type = $"{newQty} {originalUnit}".Trim();
                            
                            // Update total price based on new quantity
                            // Assuming price is per unit. 
                            originalSale.total_price = (originalSale.price * newQty) - originalSale.discount;
                            _context.sales.Update(originalSale);

                            // Create return record
                            returnsToInsert.Add(new ReturnDetail
                            {
                                SaleId = originalSale.id,
                                CustomerName = customer_name,
                                ProdNameService = item.prod_name_service,
                                Barcode = item.barcode, // Assuming item has barcode from input or lookup
                                QtyUnitType = $"{Math.Abs(qty)} {unit}".Trim(), // Reconstruct qty string
                                Amount = Math.Abs(item.total_price),
                                Date = now,
                                Method = "Refund", // Default method
                                Status = "Returned"
                            });
                        }
                        else
                        {
                            // Original sale not found, but we still want to record the return in the ledger
                            // We can add a negative sale record to keep the ledger balanced if desired, 
                            // or just add a ReturnDetail (which validates the return happened).
                            // User asked to "change in sale index", so let's add a negative sale record 
                            // if we can't find an original to update.
                            
                            item.total_price = (item.price * qty) - item.discount; // Negative total
                            salesToInsert.Add(item);

                             // Also create return record for the separate returns table
                            returnsToInsert.Add(new ReturnDetail
                            {
                                SaleId = 0, // No original sale found
                                CustomerName = customer_name,
                                ProdNameService = item.prod_name_service,
                                Barcode = item.barcode,
                                QtyUnitType = $"{Math.Abs(qty)} {unit}".Trim(),
                                Amount = Math.Abs(item.total_price),
                                Date = now,
                                Method = "Refund",
                                Status = "Returned"
                            });
                        }
                    }
                    else
                    {
                        salesToInsert.Add(item);
                    }

                    // Update Inventory
                    var product = _context.products_services.FirstOrDefault(p => p.prod_name_service == item.prod_name_service);
                    if (product != null && !string.IsNullOrEmpty(product.qty_unit_type))
                    {
                        var prodMatch = System.Text.RegularExpressions.Regex.Match(product.qty_unit_type.Trim(), @"^([0-9.-]+)\s*(.*)$");
                        if (prodMatch.Success && decimal.TryParse(prodMatch.Groups[1].Value, out decimal currentQty))
                        {
                            string prodUnit = prodMatch.Groups[2].Value;
                            if (!string.IsNullOrEmpty(prodUnit)) prodUnit = " " + prodUnit;
                            
                            // Check for Stock Limit if selling (qty > 0)
                            if (qty > 0 && currentQty < qty)
                            {
                                ModelState.AddModelError("", $"Insufficient stock for {item.prod_name_service}. Available: {currentQty}, Requested: {qty}");
                                
                                // Reload lists
                                ViewBag.Customers = _context.customers.Where(c => c.status == "Active").ToList();
                                ViewBag.Products = _context.products_services.Where(p => p.status == "Available").ToList();
                                return View();
                            }

                            currentQty -= qty; // If qty is negative (return), this adds to stock.
                            
                            product.qty_unit_type = $"{currentQty}{prodUnit}";
                            _context.products_services.Update(product);
                        }
                    }
                }
                
                if (salesToInsert.Any()) _context.sales.AddRange(salesToInsert);
                if (returnsToInsert.Any()) _context.returns.AddRange(returnsToInsert);
                
                _context.SaveChanges();
                TempData["Success"] = "Transaction processed successfully!";
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

        [HttpPost]
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteReturn(int id)
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
