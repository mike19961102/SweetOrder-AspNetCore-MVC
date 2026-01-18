using Microsoft.AspNetCore.Mvc;
using SweetOrder.Data;
using SweetOrder.Models;
using SweetOrder.Helpers;
using Microsoft.EntityFrameworkCore;

namespace SweetOrder.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Cart/Index
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null) cart = new List<CartItem>();

            ViewBag.TotalAmount = cart.Sum(item => item.SubTotal);
            return View(cart);
        }

        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == id);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl ?? "",
                    Quantity = 1
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Cart/Checkout
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // POST: /Cart/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(string shippingAddress)
        {
            // 1. 檢查購物車
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any()) return RedirectToAction("Index");

            // 2. 檢查地址 (後端驗證)
            if (string.IsNullOrEmpty(shippingAddress))
            {
                // 如果沒填地址，暫時先跳回購物車 (或是可以回傳錯誤訊息)
                return RedirectToAction("Checkout");
            }

            try
            {
                // 3. 找會員或建立新會員
                // ★★★ 確保這裡有引用 Microsoft.EntityFrameworkCore ★★★
                var user = await _context.UserProfiles.FirstOrDefaultAsync();

                if (user == null)
                {
                    user = new User
                    {
                        EmailAddress = "demo@sweetorder.com",
                        FullName = "測試買家",
                        PasswordHash = "123456",
                        Address = "測試地址"
                    };
                    _context.UserProfiles.Add(user);
                    // 先存檔，確保 User 有 ID
                    await _context.SaveChangesAsync();
                }

                // 4. 建立訂單
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    Status = "處理中",
                    ShippingAddress = shippingAddress,
                    TotalAmount = cart.Sum(item => item.SubTotal),
                    UserId = user.Id
                };

                _context.Orders.Add(order);
                // 這裡存檔會產生 Order ID
                await _context.SaveChangesAsync();

                
                foreach (var item in cart)
                {
                    var detail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    };
                    _context.OrderDetails.Add(detail);
                }

                
                await _context.SaveChangesAsync();

                
                HttpContext.Session.Remove("Cart");
                return RedirectToAction("OrderConfirmation", new { id = order.Id });
            }
            catch (Exception ex)
            {
                
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += "\n\n詳細錯誤 (Inner Exception): " + ex.InnerException.Message;
                }
                
                return Content($"結帳失敗！請截圖給工程師看：\n{errorMessage}");
            }
        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
