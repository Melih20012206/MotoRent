using Microsoft.AspNetCore.Identity;

namespace MotoRent.Web.Models;

public class FavoriteListing
{
    public string UserId { get; set; } = null!;
    public IdentityUser User { get; set; } = null!;

    public int RentalListingId { get; set; }
    public RentalListing RentalListing { get; set; } = null!;
}