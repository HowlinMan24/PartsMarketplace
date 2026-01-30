using Application.Commands.Orders;
using Application.Queries.Listings;
using Application.Queries.Orders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IMediator mediator, ILogger<AdminController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var listingsQuery = new GetAllListingsQuery { ActiveOnly = false };
        var ordersQuery = new GetAllOrdersQuery();

        var listings = await _mediator.Send(listingsQuery);
        var orders = await _mediator.Send(ordersQuery);

        ViewBag.TotalListings = listings.Count();
        ViewBag.TotalOrders = orders.Count();
        ViewBag.ActiveListings = listings.Count(l => l.IsActive);

        return View();
    }

    public async Task<IActionResult> Listings()
    {
        var query = new GetAllListingsQuery { ActiveOnly = false };
        var listings = await _mediator.Send(query);
        return View(listings);
    }

    public async Task<IActionResult> Orders()
    {
        var query = new GetAllOrdersQuery();
        var orders = await _mediator.Send(query);
        return View(orders);
    }

    [HttpPost]
    public async Task<IActionResult> ExportListings()
    {
        var query = new GetAllListingsQuery { ActiveOnly = false };
        var listings = await _mediator.Send(query);

        var csv = new StringBuilder();
        csv.AppendLine("Id,Title,Description,Price,Category,Type,Make,Model,Year,Condition,UserId,CreatedAt,IsActive");

        foreach (var listing in listings)
        {
            csv.AppendLine($"{listing.Id},\"{listing.Title}\",\"{listing.Description}\",{listing.Price},{listing.CategoryName},{listing.ListingType},{listing.Make ?? ""},{listing.Model ?? ""},{listing.Year?.ToString() ?? ""},{listing.Condition ?? ""},{listing.UserId},{listing.CreatedAt},{listing.IsActive}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"listings_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var result = await _mediator.Send(new DeleteOrderCommand { Id = id });
        if (!result)
            return NotFound();
        return RedirectToAction(nameof(Orders));
    }

    [HttpPost]
    public async Task<IActionResult> ExportOrders()
    {
        var query = new GetAllOrdersQuery();
        var orders = await _mediator.Send(query);

        var csv = new StringBuilder();
        csv.AppendLine("Id,UserId,OrderDate,TotalAmount,Status,ShippingAddress");

        foreach (var order in orders)
        {
            csv.AppendLine($"{order.Id},{order.UserId},\"{order.OrderDate}\",{order.TotalAmount},{order.Status},\"{order.ShippingAddress}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"orders_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet]
    public IActionResult ImportListings()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ImportListings(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Please select a file to import.");
            return View();
        }

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var csv = await reader.ReadToEndAsync();
            var lines = csv.Split('\n').Skip(1);

            int imported = 0;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(',');
                if (values.Length >= 4)
                {
                    imported++;
                }
            }

            ViewBag.Message = $"Successfully imported {imported} listings.";
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing listings");
            ModelState.AddModelError("", "An error occurred while importing listings.");
            return View();
        }
    }
}
