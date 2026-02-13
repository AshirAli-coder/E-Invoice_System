using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace E_Invoice_system.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Products & Services";
            var products = _context.products_services.ToList();
            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Product/Service";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductService product, IFormFile? imageFile)
        {
            // Handle Image Upload
            if (imageFile != null && imageFile.Length > 0)
            {
                string storagePath = @"D:\netcore\E-Invoice_system\bin\Debug\images";
                if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string path = Path.Combine(storagePath, fileName);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                product.image = fileName;
            }
            // Auto-generate barcode if empty
            if (string.IsNullOrWhiteSpace(product.barcode))
            {
                var uniqueBarcode = GenerateUniqueBarcode();
                product.barcode = uniqueBarcode;
            }

            // Server-side validation for quantity 0
            if (!string.IsNullOrEmpty(product.qty_unit_type))
            {
                var qtyValue = product.qty_unit_type.Trim();
                if (qtyValue == "0" || qtyValue.StartsWith("0 "))
                {
                    product.price = 0;
                    product.discount = 0;
                    product.status = "Out of Stock";
                }
            }

            // Validate discount cannot exceed price
            if (product.discount > product.price)
            {
                ModelState.AddModelError("discount", "Discount cannot be greater than the price.");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                // Ensure description is null if empty
                if (string.IsNullOrWhiteSpace(product.description)) product.description = null;

                product.date = DateTime.Now;
                _context.products_services.Add(product);
                _context.SaveChanges();
                TempData["Success"] = "Product/Service created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        private string GenerateUniqueBarcode()
        {
            var lastProductWithBarcode = _context.products_services
                .Where(p => p.barcode != null && p.barcode.StartsWith("P"))
                .AsEnumerable() // Client-side evaluation for complex parsing if needed
                .LastOrDefault(); // Assuming simple increment, but simpler approach:
                
            // Better approach for scaling: query max
            // Since we can't easily do string-number sorting in SQL with "P", we might need to fetch all or use raw SQL.
            // For now, I'll fetch recent ones or just all barcodes starting with P to find max.
            // Given "P0000000001", it's length 11.
            
            var barcodes = _context.products_services
                .Where(p => p.barcode != null && p.barcode.StartsWith("P"))
                .Select(p => p.barcode)
                .ToList();

            long maxId = 0;
            foreach (var code in barcodes)
            {
                if (code.Length > 1 && long.TryParse(code.Substring(1), out long currentId))
                {
                    if (currentId > maxId) maxId = currentId;
                }
            }
            
            return $"P{(maxId + 1):D10}";
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.products_services.Find(id);
            if (product == null) return NotFound();
            ViewData["Title"] = "Edit Product/Service";
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductService product, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                string storagePath = @"D:\netcore\E-Invoice_system\bin\Debug\images";
                if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string path = Path.Combine(storagePath, fileName);

                // Delete old image if exists
                if (!string.IsNullOrEmpty(product.image))
                {
                    var oldPath = product.image;
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                product.image = fileName;
            }
            // Auto-generate barcode if empty on edit
            if (string.IsNullOrWhiteSpace(product.barcode))
            {
                 var uniqueBarcode = GenerateUniqueBarcode();
                 product.barcode = uniqueBarcode;
            }

            // Server-side validation for quantity 0
            if (!string.IsNullOrEmpty(product.qty_unit_type))
            {
                var qtyValue = product.qty_unit_type.Trim();
                if (qtyValue == "0" || qtyValue.StartsWith("0 "))
                {
                    product.price = 0;
                    product.discount = 0;
                    product.status = "Out of Stock";
                }
            }

            // Validate discount cannot exceed price
            if (product.discount > product.price)
            {
                ModelState.AddModelError("discount", "Discount cannot be greater than the price.");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                // Ensure description is null if empty
                if (string.IsNullOrWhiteSpace(product.description)) product.description = null;

                _context.products_services.Update(product);
                _context.SaveChanges();
                TempData["Success"] = "Product/Service updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var product = _context.products_services.Find(id);
            if (product != null)
            {
                _context.products_services.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
