using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers;

[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;
    public CategoryController(ApplicationDbContext context) => _context = context;

    [AllowAnonymous]
    public async Task<IActionResult> Index() => View(await _context.Categories.ToListAsync());

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var category = await _context.Categories.FirstOrDefaultAsync(m => m.CategoryId == id);
        return category == null ? NotFound() : View(category);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var category = await _context.Categories.FindAsync(id);
        return category == null ? NotFound() : View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.CategoryId) return NotFound();
        if (ModelState.IsValid)
        {
            _context.Update(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var category = await _context.Categories.FirstOrDefaultAsync(m => m.CategoryId == id);
        return category == null ? NotFound() : View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null) _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}