namespace MotoRent.Web.Models;

public class RentalListingDetailsViewModel
{
    public required RentalListing Listing { get; init; }

    public required IReadOnlyList<RentalAvailabilityDay> Availability { get; init; }
}

public record RentalAvailabilityDay(DateOnly Date, bool IsBooked);