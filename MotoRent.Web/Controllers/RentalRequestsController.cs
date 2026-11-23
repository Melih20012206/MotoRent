using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Web.Data;
using MotoRent.Web.Models;

namespace MotoRent.Web.Controllers;

[Authorize]
public class RentalRequestsController : Controller
{
    private static readonly TimeZoneInfo BulgariaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Sofia");

    private readonly ApplicationDbContext dbContext;
    private readonly UserManager<IdentityUser> userManager;

    public RentalRequestsController(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int id)
    {
        var renterId = userManager.GetUserId(User);

        if (renterId is null)
        {
            return Forbid();
        }

        var listing = await dbContext.RentalListings
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id);

        if (listing is null)
        {
            return NotFound();
        }

        if (listing.OwnerId == renterId)
        {
            return Forbid();
        }

        return ShowCreateForm(listing, new CreateRentalRequestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int id, CreateRentalRequestViewModel input)
    {
        var renterId = userManager.GetUserId(User);

        if (renterId is null)
        {
            return Forbid();
        }

        var listing = await dbContext.RentalListings
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id);

        if (listing is null)
        {
            return NotFound();
        }

        if (listing.OwnerId == renterId)
        {
            return Forbid();
        }

        var today = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BulgariaTimeZone));

        if (input.StartDate is DateOnly selectedStart && selectedStart < today)
        {
            ModelState.AddModelError(
                nameof(input.StartDate),
                "Началната дата трябва да е днес или по-късно.");
        }

        if (input.StartDate is DateOnly start &&
            input.EndDate is DateOnly end &&
            end < start)
        {
            ModelState.AddModelError(
                nameof(input.EndDate),
                "Крайната дата трябва да е на или след началната.");
        }

        if (!ModelState.IsValid ||
            input.StartDate is not DateOnly startDate ||
            input.EndDate is not DateOnly endDate)
        {
            return ShowCreateForm(listing, input);
        }

        dbContext.RentalRequests.Add(new RentalRequest
        {
            RentalListingId = listing.Id,
            RenterId = renterId,
            StartDate = startDate,
            EndDate = endDate,
            Status = RentalRequestStatus.Pending
        });

        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Заявката за наем е изпратена.";

        return RedirectToAction("Details", "RentalListings", new { id = listing.Id });
    }

    private IActionResult ShowCreateForm(
        RentalListing listing,
        CreateRentalRequestViewModel input)
    {
        ViewData["ListingId"] = listing.Id;
        ViewData["ListingTitle"] = $"{listing.Make} {listing.Model}";

        return View(nameof(Create), input);
    }
}