using ControlWork7.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlWork7.Controllers;

public class UsersController : Controller
{
    private readonly LibraryContext _context;

    public UsersController(LibraryContext context)
    {
        _context = context;
    }

    
    public IActionResult Register()
    {
        return View();
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(User user)
    {
        if (ModelState.IsValid)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Books");
        }
        return View(user);
    }

    
    public async Task<IActionResult> Profile(string email)
    {
        var user = await _context.Users
            .Include(u => u.Issues)
            .ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null) return NotFound();

        return View(user);
    }
   
    public IActionResult Cabinet() => View();
    
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Cabinet(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("", "Укажите email.");
            return View();
        }
        return RedirectToAction(nameof(Profile), new { email });
    }

}