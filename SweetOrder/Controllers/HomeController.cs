using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SweetOrder.Models;
using SweetOrder.Data;
using Microsoft.EntityFrameworkCore;

namespace SweetOrder.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cakes = await _context.Products
                                      .Where(p => p.IsAvailable == true)
                                      .ToListAsync();
            return View(cakes);
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

        // GET: Home/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // 去資料庫找這顆蛋糕
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            // 回傳給 View 顯示
            return View(product);
        }
    }
}
