using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers;

[Authorize]
public class BorrowingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BorrowingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var borrowings = _context.Borrowings.Include(b => b.Book).Include(b => b.User);
        return View(await borrowings.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var borrowing = await _context.Borrowings
            .Include(b => b.Book).Include(b => b.User)
            .FirstOrDefaultAsync(m => m.Id == id);
        return borrowing == null ? NotFound() : View(borrowing);
    }

    public IActionResult Create()
    {
        ViewData["BookId"] = new SelectList(
            _context.Books.Where(b => b.AvailableCopies > 0), "BookId", "Title");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Borrowing borrowing)
    {
        var book = await _context.Books.FindAsync(borrowing.BookId);
        if (book == null || book.AvailableCopies <= 0)
        {
            ModelState.AddModelError("", "Book is not available.");
        }
        else
        {
            borrowing.UserId = _userManager.GetUserId(User)!;
            borrowing.BorrowDate = DateTime.Now;
            borrowing.ReturnDate = null;

            book.AvailableCopies--;
            _context.Add(borrowing);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyBorrowings));
        }
        ViewData["BookId"] = new SelectList(
            _context.Books.Where(b => b.AvailableCopies > 0), "BookId", "Title", borrowing.BookId);
        return View(borrowing);
    }

    public async Task<IActionResult> MyBorrowings()
    {
        var userId = _userManager.GetUserId(User);
        var borrowings = _context.Borrowings
            .Include(b => b.Book)
            .Where(b => b.UserId == userId && b.ReturnDate == null);
        return View(await borrowings.ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id)
    {
        var borrowing = await _context.Borrowings
            .Include(b => b.Book)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (borrowing == null) return NotFound();
        if (borrowing.ReturnDate != null)
        {
            TempData["Error"] = "Book already returned.";
            return RedirectToAction(nameof(MyBorrowings));
        }

        borrowing.ReturnDate = DateTime.Now;
        borrowing.Book.AvailableCopies++;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(MyBorrowings));
    }
}