using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Web.Data;
using MotoRent.Web.Models;

namespace MotoRent.Web.Controllers;

[Authorize]
public class RentalListingsController : Controller
{
    private const int MinimumPublisherAge = 18;
    private static readonly TimeZoneInfo BulgariaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Sofia");

    private readonly ApplicationDbContext dbContext;
    private readonly UserManager<IdentityUser> userManager;

    public RentalListingsController(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(
        string? search,
        string? sort,
        MotorcycleCategory? category)
    {
        var query = dbContext.RentalListings.AsNoTracking();
        var term = search?.Trim();

        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(listing =>
                listing.Make.Contains(term) ||
                listing.Model.Contains(term) ||
                listing.City.Contains(term));
        }

        if (category is MotorcycleCategory selectedCategory &&
            Enum.IsDefined(selectedCategory))
        {
            query = query.Where(listing => listing.Category == selectedCategory);
            ViewData["Category"] = selectedCategory;
        }

        var selectedSort = sort switch
        {
            "price-asc" => "price-asc",
            "price-desc" => "price-desc",
            _ => "newest"
        };

        var orderedQuery = selectedSort switch
        {
            "price-asc" => query
                .OrderBy(listing => listing.DailyPrice)
                .ThenByDescending(listing => listing.Id),
            "price-desc" => query
                .OrderByDescending(listing => listing.DailyPrice)
                .ThenByDescending(listing => listing.Id),
            _ => query.OrderByDescending(listing => listing.Id)
        };

        ViewData["Search"] = term;
        ViewData["Sort"] = selectedSort;

        var listings = await orderedQuery.ToListAsync();

        return View(listings);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var listing = await dbContext.RentalListings
            .AsNoTracking()
            .SingleOrDefaultAsync(listing => listing.Id == id);

        if (listing is null)
        {
            return NotFound();
        }

        return View(listing);
    }

    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        var ownerId = userManager.GetUserId(User);

        if (ownerId is null)
        {
            return Forbid();
        }

        var listings = await dbContext.RentalListings
            .AsNoTracking()
            .Where(listing => listing.OwnerId == ownerId)
            .OrderByDescending(listing => listing.Id)
            .ToListAsync();

        return View(listings);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var ownerId = userManager.GetUserId(User);

        if (ownerId is null)
        {
            return Forbid();
        }

        var listing = await dbContext.RentalListings
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId);

        if (listing is null)
        {
            return NotFound();
        }

        ViewData["ListingId"] = listing.Id;

        return View(new CreateRentalListingViewModel
        {
            Category = listing.Category,
            Make = listing.Make,
            Model = listing.Model,
            City = listing.City,
            DailyPrice = listing.DailyPrice,
            Description = listing.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Category,Make,Model,City,DailyPrice,Description")]
        CreateRentalListingViewModel input)
    {
        var ownerId = userManager.GetUserId(User);

        if (ownerId is null)
        {
            return Forbid();
        }

        var listing = await dbContext.RentalListings
            .SingleOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId);

        if (listing is null)
        {
            return NotFound();
        }

        if (input.DailyPrice is decimal price && decimal.Round(price, 2) != price)
        {
            ModelState.AddModelError(
                nameof(input.DailyPrice),
                "Въведи цена с най-много два знака след десетичния разделител.");
        }

        if (!ModelState.IsValid || input.DailyPrice is not decimal dailyPrice)
        {
            ViewData["ListingId"] = id;
            return View(input);
        }

        listing.Category = input.Category;
        listing.Make = input.Make.Trim();
        listing.Model = input.Model.Trim();
        listing.City = input.City.Trim();
        listing.DailyPrice = dailyPrice;
        listing.Description = input.Description.Trim();

        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Промените по обявата са запазени.";
        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var ownerId = userManager.GetUserId(User);

        if (ownerId is null)
        {
            return Forbid();
        }

        var listing = await dbContext.RentalListings
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId);

        if (listing is null)
        {
            return NotFound();
        }

        return View(listing);
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ownerId = userManager.GetUserId(User);

        if (ownerId is null)
        {
            return Forbid();
        }

        var deletedCount = await dbContext.RentalListings
            .Where(item => item.Id == id && item.OwnerId == ownerId)
            .ExecuteDeleteAsync();

        if (deletedCount == 0)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Обявата е изтрита успешно.";
        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var owner = await userManager.GetUserAsync(User);

        if (owner is null)
        {
            return Forbid();
        }

        ViewData["NeedsDateOfBirth"] = dbContext.Entry(owner)
            .Property<DateOnly?>(ApplicationDbContext.DateOfBirthProperty)
            .CurrentValue is null;

        return View(new CreateRentalListingViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRentalListingViewModel input)
    {
        var owner = await userManager.GetUserAsync(User);

        if (owner is null)
        {
            return Forbid();
        }

        var storedDate = dbContext.Entry(owner)
            .Property<DateOnly?>(ApplicationDbContext.DateOfBirthProperty);
        var needsDateOfBirth = storedDate.CurrentValue is null;
        ViewData["NeedsDateOfBirth"] = needsDateOfBirth;

        if (!needsDateOfBirth)
        {
            ModelState.Remove(nameof(input.DateOfBirth));
        }

        var dateOfBirth = storedDate.CurrentValue ?? input.DateOfBirth;
        var today = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BulgariaTimeZone));

        if (dateOfBirth is null)
        {
            if (ModelState.GetFieldValidationState(nameof(input.DateOfBirth)) !=
                Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
            {
                ModelState.AddModelError(
                    nameof(input.DateOfBirth),
                    "Въведи дата на раждане.");
            }
        }
        else if (dateOfBirth.Value > today.AddYears(-MinimumPublisherAge))
        {
            var errorKey = needsDateOfBirth ? nameof(input.DateOfBirth) : string.Empty;
            var message = dateOfBirth.Value > today
                ? "Датата на раждане не може да е в бъдещето."
                : "За да публикуваш обява, трябва да имаш навършени 18 години.";

            ModelState.AddModelError(errorKey, message);
        }

        if (input.DailyPrice is decimal price && decimal.Round(price, 2) != price)
        {
            ModelState.AddModelError(
                nameof(input.DailyPrice),
                "Въведи цена с най-много два знака след десетичния разделител.");
        }

        if (!ModelState.IsValid || input.DailyPrice is not decimal dailyPrice)
        {
            return View(input);
        }

        if (needsDateOfBirth)
        {
            storedDate.CurrentValue = dateOfBirth;
        }

        var listing = new RentalListing
        {
            Category = input.Category,
            Make = input.Make.Trim(),
            Model = input.Model.Trim(),
            City = input.City.Trim(),
            DailyPrice = dailyPrice,
            Description = input.Description.Trim(),
            OwnerId = owner.Id
        };

        dbContext.RentalListings.Add(listing);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (needsDateOfBirth)
        {
            ModelState.AddModelError(
                string.Empty,
                "Профилът ти беше обновен междувременно. Опитай да публикуваш отново.");

            return View(input);
        }

        TempData["SuccessMessage"] = "Обявата е публикувана успешно.";
        return RedirectToAction(nameof(Create));
    }
}