using deliveryDomain.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace deliveryInfrastructure.Controllers
{
    public class OrdersController : Controller
    {
        private readonly DeliveryBdContext _context;

        public OrdersController(DeliveryBdContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Courier)
                .Include(o => o.Payments);
            return View(await orders.ToListAsync());
        }

        //// GET: Orders/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var order = await _context.Orders
        //        .Include(o => o.Client)
        //        .Include(o => o.Courier)
        //        .Include(o => o.Payments)
        //        .FirstOrDefaultAsync(m => m.Id == id);

        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(order);
        //}


        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Courier)
                .Include(o => o.Payments)
                .Include(o => o.OrderGoods) // Додаємо товари
                .ThenInclude(og => og.Good)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Якщо статус "Виконане", зміна на "Скасоване" заборонена
            if (order.Status == "Виконане" && status == "Скасоване")
            {
                TempData["Error"] = "Неможливо скасувати виконане замовлення.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Оновлюємо статус
            if (!string.IsNullOrEmpty(status) && status != order.Status)
            {
                order.Status = status;
                _context.Update(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Статус замовлення успішно оновлено.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        public IActionResult Create()
        {
            ViewBag.Clients = new SelectList(_context.Clients, "Id", "Name");
            ViewBag.Couriers = new SelectList(_context.Couriers, "Id", "Name");
            ViewBag.Goods = new SelectList(_context.Goods, "Id", "Name");
            return View(new Order
            {
                OrderDate = DateTime.Now
            });
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ClientId,CourierId,OrderDate,DeliveryAddress")] Order? order,
            int[]? goodIds, int[]? quantities)
        {
            // Перевірка унікальності Id (якщо ви все ще хочете вручну задавати Id)
            if (_context.Orders.Any(o => o.Id == order.Id && order.Id != 0))
            {
                ModelState.AddModelError("Id", "Замовлення з таким Id уже існує.");
            }

            order.Client = _context.Clients.FirstOrDefault(c => c.Id == order.ClientId);
            order.Courier = _context.Couriers.FirstOrDefault(c => c.Id == order.CourierId);

            if (order.Client == null)
            {
                ModelState.AddModelError("ClientId", "Обраний клієнт не існує.");
            }
            if (order.Courier == null)
            {
                ModelState.AddModelError("CourierId", "Обраний кур’єр не існує.");
            }

            // Встановлюємо статус за замовчуванням
            order.Status = "Нове";
            order.OrderGoods = new List<OrderGood>(); // Ініціалізуємо порожній список

            // Перевірка наявності товарів
            if (goodIds == null || quantities == null || goodIds.Length != quantities.Length || !goodIds.Any(g => g > 0))
            {
                ModelState.AddModelError("", "Додайте хоча б один товар до замовлення.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Крок 1: Додаємо замовлення до бази даних без товарів
                    _context.Add(order);
                    await _context.SaveChangesAsync(); // Зберігаємо, щоб отримати згенерований Id

                    //Крок 2: Додаємо товари до замовлення
                    decimal totalAmount = 0;
                    if (goodIds != null && quantities != null && goodIds.Length == quantities.Length)
                    {
                        for (int i = 0; i < goodIds.Length; i++)
                        {
                            if (goodIds[i] > 0 && quantities[i] > 0)
                            {
                                var good = await _context.Goods.FindAsync(goodIds[i]);
                                if (good != null)
                                {
                                    var orderGood = new OrderGood
                                    {
                                        OrderId = order.Id, // Використовуємо згенерований Id
                                        GoodId = goodIds[i],
                                        Quantity = quantities[i]
                                    };
                                    order.OrderGoods.Add(orderGood);
                                    totalAmount += good.Price * quantities[i];
                                }
                            }
                        }
                    }

                    // Оновлюємо TotalAmount
                    order.TotalAmount = totalAmount;

                    // Крок 3: Оновлюємо замовлення з товарами та платежем
                    _context.Update(order);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Виникла помилка при збереженні замовлення: " + ex.Message);
                }
            }

            ViewBag.Clients = new SelectList(_context.Clients, "Id", "Name", order.ClientId);
            ViewBag.Couriers = new SelectList(_context.Couriers, "Id", "Name", order.CourierId);
            ViewBag.Goods = new SelectList(_context.Goods, "Id", "Name");
            return View(order);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Clients = new SelectList(_context.Clients, "Id", "Name", order.ClientId);
            ViewBag.Couriers = new SelectList(_context.Couriers, "Id", "Name", order.CourierId);
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,CourierId,OrderDate,TotalAmount,DeliveryAddress,Status")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Orders.Any(e => e.Id == order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Clients = new SelectList(_context.Clients, "Id", "Name", order.ClientId);
            ViewBag.Couriers = new SelectList(_context.Couriers, "Id", "Name", order.CourierId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Courier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                // Видаляємо залежності перед Order:
                var orderGoods = _context.OrderGoods.Where(og => og.OrderId == id);
                _context.OrderGoods.RemoveRange(orderGoods);

                if (order.Payments != null && order.Payments.Any())
                {
                    _context.Payments.RemoveRange(order.Payments);
                }

                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }



    }
}
