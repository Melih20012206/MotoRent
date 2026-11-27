using System.ComponentModel.DataAnnotations;

namespace MotoRent.Web.Models;

public class RentalListingImage
{
    public int Id { get; set; }

    public int RentalListingId { get; set; }
    public RentalListing RentalListing { get; set; } = null!;

    [MaxLength(32)]
    public string ContentType { get; set; } = null!;

    public byte[] Content { get; set; } = null!;
}