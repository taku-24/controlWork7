using ControlWork7.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ControlWork7.Controllers;

public class BooksController : Controller
{
    private readonly LibraryContext _context;

    public BooksController(LibraryContext context) => _context = context;

    
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 8,
        string? title = null,
        string? author = null,
        string status = "all",
        int? categoryId = null,
        string sort = "created_desc")
    {
        var query = _context.Books
            .Include(b => b.Category)
            .Include(b => b.Issues)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(b => b.Title.Contains(title));
        if (!string.IsNullOrWhiteSpace(author))
            query = query.Where(b => b.Author.Contains(author));
        if (categoryId.HasValue)
            query = query.Where(b => b.CategoryId == categoryId.Value);
        if (status == "available")
            query = query.Where(b => b.Issues.All(i => i.ReturnedAt != null));
        else if (status == "issued")
            query = query.Where(b => b.Issues.Any(i => i.ReturnedAt == null));

        query = sort switch
        {
            "title_asc"   => query.OrderBy(b => b.Title),
            "title_desc"  => query.OrderByDescending(b => b.Title),
            "author_asc"  => query.OrderBy(b => b.Author),
            "author_desc" => query.OrderByDescending(b => b.Author),
            "status_asc"  => query.OrderBy(b => b.Issues.Any(i => i.ReturnedAt == null)),
            "status_desc" => query.OrderByDescending(b => b.Issues.Any(i => i.ReturnedAt == null)),
            _             => query.OrderByDescending(b => b.CreatedAt)
        };

        var total = await query.CountAsync();
        var books = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var categoriesList = await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();

        var vm = new BookListViewModel
        {
            Books = books,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            TitleFilter = title,
            AuthorFilter = author,
            StatusFilter = status,
            CategoryId = categoryId,
            Sort = sort,
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync()
        };

        ViewBag.Categories = categoriesList;

        return View(vm);
    }

    
    public async Task<IActionResult> Details(int id)
    {
        var book = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.Issues).ThenInclude(i => i.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null) return NotFound();
        return View(book);
    }

    
    public IActionResult Create()
    {
        ViewBag.Categories = _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();

        return View();
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book)
    {
        ModelState.Remove(nameof(book.Category));

        ViewBag.Categories = _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();

        if (!ModelState.IsValid)
            return View(book);

        if (book.CreatedAt == default)
            book.CreatedAt = DateTime.UtcNow;
        else
            book.CreatedAt = DateTime.SpecifyKind(book.CreatedAt, DateTimeKind.Utc);

        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    
    public async Task<IActionResult> Edit(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();

        ViewBag.Categories = _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();

        return View(book);
    }

   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Book book)
    {
        ModelState.Remove(nameof(book.Category));

        ViewBag.Categories = _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();

        if (!ModelState.IsValid)
            return View(book);

        book.CreatedAt = DateTime.SpecifyKind(book.CreatedAt, DateTimeKind.Utc);

        try
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Books.AnyAsync(b => b.Id == book.Id))
                return NotFound();
            throw;
        }
    }

    
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.Books
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (book == null) return NotFound();
        return View(book);
    }

    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Take(int id, string email)
    {
        var user = await _context.Users
            .Include(u => u.Issues)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            TempData["Error"] = "Пользователь с таким email не найден.";
            return RedirectToAction("Details", new { id });
        }

        if (user.Issues != null && user.Issues.Count(i => i.ReturnedAt == null) >= 3)
        {
            TempData["Error"] = "Пользователь не может взять больше 3 книг.";
            return RedirectToAction("Details", new { id });
        }

        bool alreadyIssued = await _context.Issues
            .AnyAsync(i => i.BookId == id && i.ReturnedAt == null);
        if (alreadyIssued)
        {
            TempData["Error"] = "Книга уже выдана.";
            return RedirectToAction("Details", new { id });
        }

        var issue = new Issue
        {
            BookId = id,
            UserId = user.Id,
            IssuedAt = DateTime.UtcNow
        };

        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}