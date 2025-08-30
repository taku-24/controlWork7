using ControlWork7.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlWork7.Controllers;

public class CategoriesController : Controller
{
    private readonly LibraryContext _context;
    public CategoriesController(LibraryContext context) => _context = context;

    public async Task<IActionResult> Index()
        => View(await _context.Categories.OrderBy(c => c.Name).ToListAsync());

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid) return View(category);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        return category == null ? NotFound() : View(category);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category category)
    {
        if (!ModelState.IsValid) return View(category);
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        return category == null ? NotFound() : View(category);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}