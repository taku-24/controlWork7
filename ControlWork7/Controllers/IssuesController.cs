using ControlWork7.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlWork7.Controllers;

public class IssuesController : Controller
{
    private readonly LibraryContext _context;

    public IssuesController(LibraryContext context)
    {
        _context = context;
    }

    
    public async Task<IActionResult> Index()
    {
        var issues = await _context.Issues
            .Include(i => i.Book)
            .Include(i => i.User)
            .ToListAsync();

        return View("~/Views/Users/Index.cshtml", issues);

    }

    
    [HttpPost]
    public async Task<IActionResult> Return(int id)
    {
        var issue = await _context.Issues.FindAsync(id);
        if (issue == null) return NotFound();
        
        issue.ReturnedAt = DateTime.UtcNow; 
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}