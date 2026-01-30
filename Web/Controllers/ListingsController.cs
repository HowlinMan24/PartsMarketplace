using Application.Commands.Listings;
using Application.DTOs;
using Application.Queries.Listings;
using Application.Queries.Pricing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;
using System.Security.Claims;
using Domain.Interfaces;
using Infrastructure.Services;

namespace Web.Controllers;

public class ListingsController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<ListingsController> _logger;
    private readonly IGeolocationService _geolocationService;
    private readonly ICurrencyService _currencyService;

    public ListingsController(
        IMediator mediator, 
        ILogger<ListingsController> logger,
        IGeolocationService geolocationService,
        ICurrencyService currencyService)
    {
        _mediator = mediator;
        _logger = logger;
        _geolocationService = geolocationService;
        _currencyService = currencyService;
    }

    [Authorize]
    public async Task<IActionResult> Index(string? category, string? listingType)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var query = new GetAllListingsQuery
        {
            ActiveOnly = true,
            Category = category,
            ListingType = listingType,
            UserId = userId
        };

        var listings = await _mediator.Send(query);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userCurrency = await _geolocationService.GetCurrencyCodeAsync(ipAddress ?? "");

        var listingViewModels = new List<ListingIndexItemViewModel>();
        foreach (var listing in listings)
        {
            var convertedPrice = await _currencyService.ConvertAsync(listing.Price, listing.Currency, userCurrency);

            var priceTransformation = new PriceTransformationDto
            {
                OriginalPrice = listing.Price,
                ExchangeRate = convertedPrice / (listing.Price == 0 ? 1 : listing.Price),
                PriceInMKD = convertedPrice,
                TaxAmount = 0,
                TotalPriceWithTax = convertedPrice,
                Currency = userCurrency
            };

            listingViewModels.Add(new ListingIndexItemViewModel
            {
                Listing = listing,
                PriceTransformation = priceTransformation
            });
        }

        return View(listingViewModels);
    }

    public async Task<IActionResult> Details(int id)
    {
        var query = new GetListingByIdQuery { Id = id };
        var listing = await _mediator.Send(query);

        if (listing == null)
        {
            return NotFound();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userCurrency = await _geolocationService.GetCurrencyCodeAsync(ipAddress ?? "");

        var convertedPrice = await _currencyService.ConvertAsync(listing.Price, listing.Currency, userCurrency);

        var priceTransformation = new PriceTransformationDto
        {
            OriginalPrice = listing.Price,
            ExchangeRate = convertedPrice / (listing.Price == 0 ? 1 : listing.Price),
            PriceInMKD = convertedPrice,
            TaxAmount = 0,
            TotalPriceWithTax = convertedPrice,
            Currency = userCurrency
        };

        var viewModel = new ListingDetailsViewModel
        {
            Listing = listing,
            PriceTransformation = priceTransformation
        };

        return View(viewModel);
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Create()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userCurrency = await _geolocationService.GetCurrencyCodeAsync(ipAddress ?? "");
        
        var model = new CreateListingDto
        {
             Currency = userCurrency
        };
        
        return View(model);
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateListingDto listingDto)
    {
        if (ModelState.IsValid)
        {
            listingDto.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var command = new CreateListingCommand { Listing = listingDto };
            var result = await _mediator.Send(command);
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }

        return View(listingDto);
    }

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var query = new GetListingByIdQuery { Id = id };
        var listing = await _mediator.Send(query);

        if (listing == null)
        {
            return NotFound();
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId) || (listing.UserId != currentUserId && !User.IsInRole("Admin")))
            return Forbid();

        ViewBag.ListingId = id;
        var editDto = new CreateListingDto
        {
            Title = listing.Title,
            Description = listing.Description,
            Price = listing.Price,
            CategoryId = listing.CategoryId,
            ListingType = listing.ListingType,
            Make = listing.Make,
            Model = listing.Model,
            Year = listing.Year,
            Condition = listing.Condition,
            ImageUrl = listing.ImageUrl,
            UserId = listing.UserId
        };

        return View(editDto);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateListingDto listingDto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var existingListing = await _mediator.Send(new GetListingByIdQuery { Id = id });
                if (existingListing == null)
                {
                    return NotFound();
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId) || (existingListing.UserId != currentUserId && !User.IsInRole("Admin")))
                {
                    return Forbid();
                }

                var command = new UpdateListingCommand
                {
                    Id = id,
                    Listing = listingDto
                };
                var result = await _mediator.Send(command);
                return RedirectToAction(nameof(Details), new { id = result.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating listing {ListingId}", id);
                ModelState.AddModelError("", "An error occurred while updating the listing.");
            }
        }

        ViewBag.ListingId = id;
        return View(listingDto);
    }

    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var query = new GetListingByIdQuery { Id = id };
        var listing = await _mediator.Send(query);

        if (listing == null)
        {
            return NotFound();
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId) || (listing.UserId != currentUserId && !User.IsInRole("Admin")))
        {
            return Forbid();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userCurrency = await _geolocationService.GetCurrencyCodeAsync(ipAddress ?? "");

        var convertedPrice = await _currencyService.ConvertAsync(listing.Price, listing.Currency, userCurrency);

        var priceTransformation = new PriceTransformationDto
        {
            OriginalPrice = listing.Price,
            ExchangeRate = convertedPrice / (listing.Price == 0 ? 1 : listing.Price),
            PriceInMKD = convertedPrice,
            TaxAmount = 0,
            TotalPriceWithTax = convertedPrice,
            Currency = userCurrency
        };

        var viewModel = new ListingDetailsViewModel
        {
            Listing = listing,
            PriceTransformation = priceTransformation
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var listing = await _mediator.Send(new GetListingByIdQuery { Id = id });
        if (listing == null)
        {
            return NotFound();
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId) || (listing.UserId != currentUserId && !User.IsInRole("Admin")))
        {
            return Forbid();
        }

        try
        {
            var command = new DeleteListingCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
