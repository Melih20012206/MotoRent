using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MotoRent.Web.Data;
using MotoRent.Web.Models;

namespace MotoRent.Web.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly ApplicationDbContext dbContext;
    private readonly UserManager<IdentityUser> userManager;

    public FavoritesController(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User);

        var listings = await dbContext.FavoriteListings
            .AsNoTracking()
            .Where(favorite => favorite.UserId == userId)
            .OrderBy(favorite => favorite.RentalListing.Make)
            .ThenBy(favorite => favorite.RentalListing.Model)
            .Select(favorite => favorite.RentalListing)
            .ToListAsync();

        return View(listings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int id)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        if (!await dbContext.RentalListings.AnyAsync(listing => listing.Id == id))
        {
            return NotFound();
        }

        var alreadySaved = await dbContext.FavoriteListings.AnyAsync(favorite =>
            favorite.UserId == userId && favorite.RentalListingId == id);

        if (!alreadySaved)
        {
            dbContext.FavoriteListings.Add(new FavoriteListing
            {
                UserId = userId,
                RentalListingId = id
            });

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is SqlException { Number: 2627 or 2601 })
            {
                // A concurrent request already saved this favorite.
            }
        }

        return RedirectToAction("Details", "RentalListings", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id, bool fromFavorites = false)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        await dbContext.FavoriteListings
            .Where(favorite => favorite.UserId == userId && favorite.RentalListingId == id)
            .ExecuteDeleteAsync();

        if (fromFavorites)
        {
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction("Details", "RentalListings", new { id });
    }
}