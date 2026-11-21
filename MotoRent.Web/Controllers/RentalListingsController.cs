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
    public async Task<IActionResult> Index()
    {
        var listings = await dbContext.RentalListings
            .AsNoTracking()
            .OrderByDescending(listing => listing.Id)
            .ToListAsync();

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
    public IActionResult Create()
    {
        return View(new CreateRentalListingViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRentalListingViewModel input)
    {
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

        var owner = await userManager.GetUserAsync(User);

        if (owner is null)
        {
            return Forbid();
        }

        var listing = new RentalListing
        {
            Make = input.Make.Trim(),
            Model = input.Model.Trim(),
            City = input.City.Trim(),
            DailyPrice = dailyPrice,
            Description = input.Description.Trim(),
            OwnerId = owner.Id
        };

        dbContext.RentalListings.Add(listing);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Обявата е публикувана успешно.";
        return RedirectToAction(nameof(Create));
    }
}