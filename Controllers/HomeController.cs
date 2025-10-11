using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartShop.Models;
using SmartShop.Models.Data;
using System.Collections.Generic;

namespace SmartShop.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;

        public HomeController(ILogger<HomeController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products
                           .Include(p => p.Category)
                           .ToList();

            // فقط لو مفيش أي منتجات في الداتابيز
            if (!products.Any())
            {
                products = new List<Product>
        {
            new Product {
                Id = 1,
                Title = "iPhone 13",
                Price = 999.99m,
                ImageUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=400",
                Description = "أحدث هاتف من Apple"
            },
            new Product {
                Id = 2,
                Title = "Samsung Galaxy",
                Price = 799.99m,
                ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400",
                Description = "هاتف أندرويد متطور"
            }
        };
            }

            return View(products);
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