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

        var datesAreBooked = await dbContext.RentalRequests.AnyAsync(request =>
            request.RentalListingId == listing.Id &&
            request.Status == RentalRequestStatus.Accepted &&
            request.StartDate <= endDate &&
            request.EndDate >= startDate);

        if (datesAreBooked)
        {
            ModelState.AddModelError(
                nameof(input.StartDate),
                "Моторът вече е резервиран за част от избрания период.");

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

    [HttpGet]
    public async Task<IActionResult> Incoming()
    {
        var ownerId = userManager.GetUserId(User);

        if (ownerId is null)
        {
            return Forbid();
        }

        var requests = await dbContext.RentalRequests
            .AsNoTracking()
            .Include(request => request.RentalListing)
            .Where(request => request.RentalListing.OwnerId == ownerId)
            .OrderByDescending(request => request.Id)
            .ToListAsync();

        return View(requests);
    }

    [HttpGet]
    public async Task<IActionResult> Outgoing()
    {
        var renterId = userManager.GetUserId(User);

        if (renterId is null)
        {
            return Forbid();
        }

        var requests = await dbContext.RentalRequests
            .AsNoTracking()
            .Include(request => request.RentalListing)
            .Where(request => request.RenterId == renterId)
            .OrderByDescending(request => request.Id)
            .ToListAsync();

        return View(requests);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var renterId = userManager.GetUserId(User);

        if (renterId is null)
        {
            return Forbid();
        }

        var updatedCount = await dbContext.RentalRequests
            .Where(request =>
                request.Id == id &&
                request.RenterId == renterId &&
                request.Status == RentalRequestStatus.Pending)
            .ExecuteUpdateAsync(setters => setters.SetProperty(
                request => request.Status,
                RentalRequestStatus.Cancelled));

        if (updatedCount == 0)
        {
            TempData["ErrorMessage"] = "Заявката вече не може да бъде оттеглена.";
        }
        else
        {
            TempData["SuccessMessage"] = "Заявката е оттеглена.";
        }

        return RedirectToAction(nameof(Outgoing));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Accept(int id)
    {
        return UpdateStatusAsync(id, RentalRequestStatus.Accepted);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Reject(int id)
    {
        return UpdateStatusAsync(id, RentalRequestStatus.Rejected);
    }

    private async Task<IActionResult> UpdateStatusAsync(
        int id,
        RentalRequestStatus newStatus)
    {
        var ownerId = userManager.GetUserId(User);

        if (ownerId is null)
        {
            return Forbid();
        }

        var pendingRequest = dbContext.RentalRequests.Where(request =>
            request.Id == id &&
            request.RentalListing.OwnerId == ownerId &&
            request.Status == RentalRequestStatus.Pending);

        var updatedCount = 0;

        try
        {
            if (newStatus == RentalRequestStatus.Accepted)
            {
                await using var transaction = await dbContext.Database
                    .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                updatedCount = await pendingRequest
                    .Where(request => !dbContext.RentalRequests.Any(other =>
                        other.RentalListingId == request.RentalListingId &&
                        other.Status == RentalRequestStatus.Accepted &&
                        other.StartDate <= request.EndDate &&
                        other.EndDate >= request.StartDate))
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(request => request.Status, newStatus));

                await transaction.CommitAsync();
            }
            else
            {
                updatedCount = await pendingRequest.ExecuteUpdateAsync(setters =>
                    setters.SetProperty(request => request.Status, newStatus));
            }
        }
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 1205)
        {
            TempData["ErrorMessage"] = "Има едновременна промяна. Опитай отново.";
            return RedirectToAction(nameof(Incoming));
        }

        if (updatedCount == 0)
        {
            TempData["ErrorMessage"] = newStatus == RentalRequestStatus.Accepted
                ? "Заявката вече не чака отговор, не е за твоя обява или датите са заети."
                : "Заявката вече не чака отговор или не е за твоя обява.";
        }
        else
        {
            TempData["SuccessMessage"] = newStatus == RentalRequestStatus.Accepted
                ? "Заявката е приета."
                : "Заявката е отказана.";
        }

        return RedirectToAction(nameof(Incoming));
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