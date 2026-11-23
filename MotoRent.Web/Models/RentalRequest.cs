using Microsoft.AspNetCore.Identity;

namespace MotoRent.Web.Models;

public class RentalRequest
{
    public int Id { get; set; }

    public int RentalListingId { get; set; }
    public RentalListing RentalListing { get; set; } = null!;

    public string RenterId { get; set; } = null!;
    public IdentityUser Renter { get; set; } = null!;

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public RentalRequestStatus Status { get; set; } = RentalRequestStatus.Pending;
}