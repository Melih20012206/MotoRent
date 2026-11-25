using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Web.Data;
using MotoRent.Web.Models;

namespace MotoRent.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Administrator")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext dbContext;

    public DashboardController(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            TotalListings = await dbContext.RentalListings.CountAsync(),
            PendingRequests = await dbContext.RentalRequests.CountAsync(
                request => request.Status == RentalRequestStatus.Pending)
        };

        return View(model);
    }
}