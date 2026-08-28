using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers;

public class BookController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public BookController(
        ApplicationDbContext context,
        IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: Book
    public async Task<IActionResult> Index()
    {
        var books = _context.Books
            .Include(b => b.Category)
            .Include(b => b.Author);

        return View(await books.ToListAsync());
    }

    // GET: Book/Available
    public async Task<IActionResult> Available()
    {
        var books = _context.Books
            .Include(b => b.Category)
            .Include(b => b.Author)
            .Where(b => b.AvailableCopies > 0);

        return View("Index", await books.ToListAsync());
    }

    // GET: Book/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var book = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.Author)
            .FirstOrDefaultAsync(m => m.BookId == id);

        if (book == null)
            return NotFound();

        return View(book);
    }

    // GET: Book/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        ViewData["CategoryId"] = new SelectList(
            _context.Categories,
            "CategoryId",
            "Name"
        );

        ViewData["AuthorId"] = new SelectList(
            _context.Authors,
            "AuthorId",
            "FirstName"
        );

        return View();
    }

    // POST: Book/Create
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book)
    {
        // Navigation Properties لا نحتاجها عند إنشاء الكتاب
        ModelState.Remove("Category");
        ModelState.Remove("Author");
        ModelState.Remove("Borrowings");

        if (ModelState.IsValid)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // إعادة تحميل القوائم إذا كان هناك خطأ
        ViewData["CategoryId"] = new SelectList(
            _context.Categories,
            "CategoryId",
            "Name",
            book.CategoryId
        );

        ViewData["AuthorId"] = new SelectList(
            _context.Authors,
            "AuthorId",
            "FirstName",
            book.AuthorId
        );

        return View(book);
    }

    // GET: Book/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var book = await _context.Books.FindAsync(id);

        if (book == null)
            return NotFound();

        ViewData["CategoryId"] = new SelectList(
            _context.Categories,
            "CategoryId",
            "Name",
            book.CategoryId
        );

        ViewData["AuthorId"] = new SelectList(
            _context.Authors,
            "AuthorId",
            "FirstName",
            book.AuthorId
        );

        return View(book);
    }

    // POST: Book/Edit/5
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book)
    {
        if (id != book.BookId)
            return NotFound();

        ModelState.Remove("Category");
        ModelState.Remove("Author");
        ModelState.Remove("Borrowings");

        if (ModelState.IsValid)
        {
            _context.Update(book);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewData["CategoryId"] = new SelectList(
            _context.Categories,
            "CategoryId",
            "Name",
            book.CategoryId
        );

        ViewData["AuthorId"] = new SelectList(
            _context.Authors,
            "AuthorId",
            "FirstName",
            book.AuthorId
        );

        return View(book);
    }

    // GET: Book/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var book = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.Author)
            .FirstOrDefaultAsync(m => m.BookId == id);

        if (book == null)
            return NotFound();

        return View(book);
    }

    // POST: Book/Delete/5
    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book != null)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Book/Read/5
    [AllowAnonymous]
    public async Task<IActionResult> Read(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null || string.IsNullOrEmpty(book.FilePath))
        {
            return NotFound("الكتاب غير متاح للقراءة حاليًا.");
        }

        return View(book);
    }

    // POST: Book/UploadFile/5
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadFile(int id, IFormFile file)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
            return NotFound();

        if (file == null || file.Length == 0)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        // التأكد أن الملف PDF
        if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var folderPath = Path.Combine(
            _env.WebRootPath,
            "BookFiles"
        );

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileName = $"{id}_{Guid.NewGuid()}.pdf";

        var fullPath = Path.Combine(
            folderPath,
            fileName
        );

        using (var stream = new FileStream(
            fullPath,
            FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        book.FilePath = $"/BookFiles/{fileName}";

        _context.Update(book);

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Details),
            new { id }
        );
    }
}