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
    public class GoodsController : Controller
    {
        private readonly DeliveryBdContext _context;

        public GoodsController(DeliveryBdContext context)
        {
            _context = context;
        }

        // GET: Goods
        public async Task<IActionResult> Index(int? id, string? name)
        {
            if (id == null) { return RedirectToAction("Index", "Categories"); }

            ViewBag.Id = id;
            ViewBag.Name = name;

            // Оновлено: додаємо ViewBag для CategoryName
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category != null)
            {
                ViewBag.CategoryName = category.CategoryName;
            }

            var goodByCategory = _context.Goods.Where(g => g.CategoryId == id).Include(g => g.Category);

            return View(await goodByCategory.ToListAsync());
        }

        // GET: Goods/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var good = await _context.Goods
                .Include(g => g.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (good == null)
            {
                return NotFound();
            }

            return View(good);
        }

        // GET: Goods/Create
        public IActionResult Create(int id, string name)
        {
            if (!_context.Categories.Any(c => c.Id == id))
            {
                return NotFound();
            }

            ViewBag.CategoryId = id;
            ViewBag.CategoryName = name;
            return View();
        }

        // POST: Goods/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price")] Good good, int categoryId)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (category == null)
            {
                return NotFound();
            }

            good.CategoryId = categoryId;


            if (ModelState.IsValid)
            {
                _context.Add(good);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Goods", new { id = categoryId, name = category.CategoryName });
            }

            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = category.CategoryName;
            return View(good);
        }


        // GET: Goods/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var good = await _context.Goods.FindAsync(id);
            if (good == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", good.CategoryId);
            return View(good);
        }

        // POST: Goods/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Price,CategoryId,Id")] Good good)
        {
            if (id != good.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(good);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GoodExists(good.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", good.CategoryId);
            return View(good);
        }

        // GET: Goods/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var good = await _context.Goods
                .Include(g => g.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (good == null)
            {
                return NotFound();
            }

            return View(good);
        }


        // POST: Goods/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var good = await _context.Goods
                .Include(g => g.OrderGoods)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (good != null)
            {
                if (good.OrderGoods.Any())
                {
                    _context.OrderGoods.RemoveRange(good.OrderGoods);
                }

                _context.Goods.Remove(good);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool GoodExists(int id)
        {
            return _context.Goods.Any(e => e.Id == id);
        }
    }
}
