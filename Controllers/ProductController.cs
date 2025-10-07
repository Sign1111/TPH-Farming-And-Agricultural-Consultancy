using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

public class ProductController : Controller
{
    private readonly IMongoCollection<Product> _products;

    public ProductController(IMongoDatabase database)
    {
        _products = database.GetCollection<Product>("Products");
    }

    // GET: Product/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Product/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.CreatedAt = DateTime.UtcNow;

        await _products.InsertOneAsync(model);

        TempData["SuccessMessage"] = "Product created successfully!";
        return RedirectToAction("Index");
    }

    // GET: Product/Index (list all products)
    public async Task<IActionResult> Index()
    {
        var allProducts = await _products.Find(_ => true).ToListAsync();
        return View(allProducts);
    }
}
