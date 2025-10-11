using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartShop.Models;
using SmartShop.Models.Data;
using System.Security.Claims;

namespace SmartShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DataContext _context;

        public ProductsController(DataContext context)
        {
            _context = context;
        }

        // Display all products
        public IActionResult Index()
        {
            var products = _context.Products
                           .Include(p => p.Category)
                           .ToList();
            return RedirectToAction("Index", "Home");
        }

        // GET: Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile productImage)
        {
            if (ModelState.IsValid)
            {
                if (productImage != null && productImage.Length > 0)
                {
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(productImage.FileName);
                    var filePath = Path.Combine(imagesFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await productImage.CopyToAsync(stream);

                    product.ImageUrl = "/images/" + fileName;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Update
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Product product, IFormFile productImage)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (productImage != null && productImage.Length > 0)
                    {
                        var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(imagesFolder))
                            Directory.CreateDirectory(imagesFolder);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(productImage.FileName);
                        var filePath = Path.Combine(imagesFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                            await productImage.CopyToAsync(stream);

                        product.ImageUrl = "/images/" + fileName;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }
                catch
                {
                    return NotFound();
                }
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.CartItems)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    TempData["Error"] = "Product not found!";
                    return RedirectToAction("Index", "Home");
                }

                if (product.CartItems != null && product.CartItems.Any())
                {
                    _context.CartItems.RemoveRange(product.CartItems);
                    await _context.SaveChangesAsync();
                }

                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting product: {ex.Message}";
            }

            return RedirectToAction("Index", "Home");
        }

        // ✅ Add to Cart - Modified
        // [Authorize] // ⬅️ Commented temporarily for testing
        public IActionResult AddToCart(int productId)
        {
            // Get logged in user email
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
            {
                // If not logged in, use Demo instead of login
                return AddToCartDemo(productId);
            }

            // Find User by Email
            var user = _context.Users
                .Include(u => u.Carts)
                .ThenInclude(c => c.CartItems)
                .FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                // If no user, create a new one
                user = new User
                {
                    Name = User.FindFirstValue(ClaimTypes.Name) ?? "User",
                    Email = userEmail,
                    Password = "default-password",
                    Role = "User"
                };
                _context.Users.Add(user);
                _context.SaveChanges();
            }

            var cart = user.Carts.LastOrDefault() ?? new Cart { UserId = user.Id };
            if (cart.Id == 0)
            {
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            var product = _context.Products.Find(productId);
            if (product == null)
            {
                TempData["Error"] = "Product not found!";
                return RedirectToAction("Index", "Home");
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    CartId = cart.Id,
                    Quantity = 1
                };
                _context.CartItems.Add(cartItem);
            }

            _context.SaveChanges();

            TempData["Success"] = $"{product.Title} added to cart successfully!";
            return RedirectToAction("Index", "Home");
        }

        // ✅ Add to Cart Demo - Modified
        public IActionResult AddToCartDemo(int productId)
        {
            var demoUser = _context.Users
                .Include(u => u.Carts)
                .ThenInclude(c => c.CartItems)
                .FirstOrDefault(u => u.Email == "demo@test.com");

            if (demoUser == null)
            {
                demoUser = new User
                {
                    Name = "Demo User",
                    Email = "demo@test.com",
                    Password = "demo123",
                    Role = "User"
                };
                _context.Users.Add(demoUser);
                _context.SaveChanges();
            }

            var cart = demoUser.Carts.LastOrDefault() ?? new Cart { UserId = demoUser.Id };
            if (cart.Id == 0)
            {
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            var product = _context.Products.Find(productId);
            if (product == null)
            {
                TempData["Error"] = "Product not found!";
                return RedirectToAction("Index", "Home");
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    CartId = cart.Id,
                    Quantity = 1
                };
                _context.CartItems.Add(cartItem);
            }

            _context.SaveChanges();

            TempData["Success"] = $"{product.Title} added to cart successfully!";
            return RedirectToAction("Index", "Home");
        }

        // 🆕 Action to display cart
        public IActionResult ViewCart()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "demo@test.com";

            var user = _context.Users
                .Include(u => u.Carts)
                .ThenInclude(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(u => u.Email == userEmail);

            if (user == null || !user.Carts.Any() || !user.Carts.Last().CartItems.Any())
            {
                ViewBag.EmptyMessage = "Your cart is empty";
                return View(new Cart { CartItems = new List<CartItem>() });
            }

            var cart = user.Carts.Last();
            return View(cart);
        }

        // 🆕 Increase product quantity in cart
        [HttpPost]
        public IActionResult IncreaseQuantity(int cartItemId)
        {
            var cartItem = _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefault(ci => ci.Id == cartItemId);

            if (cartItem != null && cartItem.Quantity < cartItem.Product.Stock)
            {
                cartItem.Quantity++;
                _context.SaveChanges();
                TempData["Success"] = "Quantity increased successfully";
            }
            else
            {
                TempData["Error"] = "Cannot increase quantity beyond available stock";
            }

            return RedirectToAction("ViewCart");
        }

        // 🆕 Decrease product quantity in cart
        [HttpPost]
        public IActionResult DecreaseQuantity(int cartItemId)
        {
            var cartItem = _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefault(ci => ci.Id == cartItemId);

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                    _context.SaveChanges();
                    TempData["Success"] = "Quantity decreased successfully";
                }
                else
                {
                    // If quantity is 1, remove the item
                    _context.CartItems.Remove(cartItem);
                    _context.SaveChanges();
                    TempData["Success"] = "Product removed from cart";
                }
            }

            return RedirectToAction("ViewCart");
        }

        // 🆕 Remove product from cart
        [HttpPost]
        public IActionResult RemoveItem(int cartItemId)
        {
            var cartItem = _context.CartItems.Find(cartItemId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
                TempData["Success"] = "Product removed from cart";
            }

            return RedirectToAction("ViewCart");
        }

        // 🆕 Clear cart
        [HttpPost]
        public IActionResult ClearCart()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "demo@test.com";

            var user = _context.Users
                .Include(u => u.Carts)
                .ThenInclude(c => c.CartItems)
                .FirstOrDefault(u => u.Email == userEmail);

            if (user != null && user.Carts.Any())
            {
                var cart = user.Carts.Last();
                _context.CartItems.RemoveRange(cart.CartItems);
                _context.SaveChanges();
                TempData["Success"] = "Cart cleared successfully";
            }

            return RedirectToAction("ViewCart");
        }

        // 🆕 Search products
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction("Index", "Home");
            }

            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Title.Contains(searchTerm) ||
                           p.Description.Contains(searchTerm) ||
                           p.Category.Name.Contains(searchTerm))
                .ToList();

            ViewBag.SearchTerm = searchTerm;
            return View("~/Views/Home/Index.cshtml", products);
        }

        // 🆕 Filter products by category
        public IActionResult Category(int categoryId)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToList();

            var category = _context.Categories.Find(categoryId);
            ViewBag.CategoryName = category?.Name ?? "Category";
            return View("~/Views/Home/Index.cshtml", products);
        }
    }
}