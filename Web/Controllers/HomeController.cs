using System.Diagnostics;
using System.Globalization;
using Application.Queries.Listings;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Web.Models;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<HomeController> _logger;
    private readonly IStringLocalizer<Web.SharedResource> _localizer;
    private readonly ICurrencyService _currencyService;
    private static readonly HashSet<string> SupportedCultures = new(StringComparer.OrdinalIgnoreCase) { "en-US", "mk-MK" };

    public HomeController(IMediator mediator, ILogger<HomeController> logger, IStringLocalizer<Web.SharedResource> localizer, ICurrencyService currencyService)
    {
        _mediator = mediator;
        _logger = logger;
        _localizer = localizer;
        _currencyService = currencyService;
    }

    [HttpGet]
    public IActionResult SetCulture(string culture, string? returnUrl = null)
    {
        if (!SupportedCultures.Contains(culture))
            culture = "en-US";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), SameSite = SameSiteMode.Lax }
        );

        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpGet]
    public IActionResult DetectLocation()
    {
        Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index(string? searchTerm, string? category, string? listingType)
    {
        var query = new GetAllListingsQuery
        {
            ActiveOnly = true,
            Category = category,
            ListingType = listingType
        };

        var listings = await _mediator.Send(query);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            listings = listings.Where(l =>
                l.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                l.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (l.Make != null && l.Make.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (l.Model != null && l.Model.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }

        var listingsList = listings.ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.Category = category;
        ViewBag.ListingType = listingType;

        var isMacedonianSite = CultureInfo.CurrentUICulture.Name.StartsWith("mk", StringComparison.OrdinalIgnoreCase);
        ViewBag.IsMacedonianSite = isMacedonianSite;

        _logger.LogInformation("Home Index - Culture: {Culture}, IsMacedonianSite: {IsMacedonian}",
            CultureInfo.CurrentUICulture.Name, isMacedonianSite);

        if (isMacedonianSite)
        {
            var convertedPrices = new Dictionary<int, decimal>();
            foreach (var listing in listingsList)
            {
                if (!string.Equals(listing.Currency, "MKD", StringComparison.OrdinalIgnoreCase))
                {
                    var mkdPrice = await _currencyService.ConvertAsync(listing.Price, listing.Currency, "MKD");
                    convertedPrices[listing.Id] = mkdPrice;
                    _logger.LogInformation("Converted listing {Id}: {OriginalPrice} {Currency} -> {MKDPrice} MKD",
                        listing.Id, listing.Price, listing.Currency, mkdPrice);
                }
                else
                {
                    convertedPrices[listing.Id] = listing.Price;
                }
            }
            ViewBag.ConvertedPrices = convertedPrices;
            _logger.LogInformation("Total converted prices: {Count}", convertedPrices.Count);
        }

        return View(listingsList);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public IActionResult DebugResources()
    {
        var a = System.Reflection.Assembly.GetExecutingAssembly();
        var names = a.GetManifestResourceNames();
        var rm = new System.Resources.ResourceManager("Web.SharedResource", a);
        var mkHome = rm.GetString("Home", new System.Globalization.CultureInfo("mk-MK"));
        var enHome = rm.GetString("Home", new System.Globalization.CultureInfo("en-US"));
        var cur = System.Globalization.CultureInfo.CurrentUICulture.Name;
        var curHome = rm.GetString("Home");
        return Content($"Manifest: {string.Join(", ", names)}\n\nmk-MK Home: {mkHome ?? "(null)"}\nen-US Home: {enHome ?? "(null)"}\nCurrent UI: {cur}\nCurrent Home: {curHome ?? "(null)"}", "text/plain");
    }
}
