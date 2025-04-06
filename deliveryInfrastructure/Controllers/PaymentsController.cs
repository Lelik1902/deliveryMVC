using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using deliveryDomain.Model;
using deliveryInfrastructure;

namespace deliveryInfrastructure.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly DeliveryBdContext _context;

        public PaymentsController(DeliveryBdContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var deliveryBdContext = _context.Payments.Include(p => p.Order);
            return View(await deliveryBdContext.ToListAsync());
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }
        // GET: Payments/Create
        public IActionResult Create()
        {
            ViewBag.OrderId = new SelectList(_context.Orders, "Id", "Id");
            ViewBag.OrdersData = _context.Orders
                .Include(o => o.OrderGoods)
                .ThenInclude(og => og.Good)
                .ToList(); // Передаємо повні об'єкти Order замість анонімного типу
            return View(new Payment
            {
                PaymentDate = DateTime.Now
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var order = await _context.Orders
                .Where(o => o.Id == orderId)
                .Select(o => new
                {
                    creationDate = o.OrderDate,
                    totalAmount = o.TotalAmount
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return Json(order);
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,PaymentDate,Amount,PaymentMethod")] Payment payment)
        {
            var order = await _context.Orders
                .Include(o => o.OrderGoods)
                .ThenInclude(og => og.Good)
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId);

            if (order == null)
            {
                ModelState.AddModelError("OrderId", "Обране замовлення не існує.");
            }
            else
            {
                // Перевірка дати оплати
                if (payment.PaymentDate < order.OrderDate)
                {
                    ModelState.AddModelError("PaymentDate", "Дата оплати не може бути раніше дати створення замовлення.");
                }

                // Автоматичне встановлення суми оплати
                payment.Amount = order.OrderGoods?.Sum(og => og.Good.Price * og.Quantity) ?? 0;
                //if (payment.Amount <= 0)
                //{
                //    ModelState.AddModelError("Amount", "Сума оплати повинна бути більшою за 0.");
                //}
            }

            if (ModelState.IsValid)
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.OrderId = new SelectList(_context.Orders, "Id", "Id", payment.OrderId);
            ViewBag.OrdersData = _context.Orders
                .Select(o => new { o.Id, o.OrderDate, o.TotalAmount })
                .ToList();
            return View(payment);
        }



        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "DeliveryAddress", payment.OrderId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,PaymentDate,Amount,PaymentMethod,Id")] Payment payment)
        {
            if (id != payment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Id))
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
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "DeliveryAddress", payment.OrderId);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}
