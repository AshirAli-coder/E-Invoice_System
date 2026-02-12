using System.Diagnostics;
using E_Invoice_system.Models;
using E_Invoice_system.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace E_Invoice_system.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // COUNTS
            ViewBag.TotalInvoices = _context.invoices.Count();
            ViewBag.TotalCustomers = _context.customers.Count();
            ViewBag.TotalProducts = _context.products_services.Count();

            // ✅ TOTAL SALES AMOUNT (Filtered: excluding 0 quantity / fully returned)
            decimal totalSales = _context.sales
                .AsEnumerable()
                .Where(s => {
                    var match = System.Text.RegularExpressions.Regex.Match(s.qty_unit_type ?? "", @"^([0-9.-]+)");
                    if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal q))
                    {
                        return q > 0;
                    }
                    return true;
                })
                .Sum(s => s.total_price);

            ViewBag.totalSales = Math.Max(0, totalSales);

            // Invoice Status Chart
            var invoiceStatusData = _context.invoices
                .GroupBy(i => i.status ?? "Pending")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.StatusLabels = invoiceStatusData.Select(x => x.Status).ToArray();
            ViewBag.StatusCounts = invoiceStatusData.Select(x => x.Count).ToArray();

            // Recent invoices
            var recentInvoices = _context.invoices
                .OrderByDescending(i => i.date)
                .Take(4)
                .ToList();

            // ✅ TREND CHART DATA (Last 7 Days)
            var trendLabels = new List<string>();
            var trendData = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                trendLabels.Add(date.ToString("MMM dd"));
                var count = _context.invoices.Count(inv => inv.date.Date == date);
                trendData.Add(count);
            }
            ViewBag.TrendLabels = trendLabels.ToArray();
            ViewBag.TrendData = trendData.ToArray();

            return View(recentInvoices);
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
    }
}
