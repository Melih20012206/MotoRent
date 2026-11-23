using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MotoRent.Web.Models;

public class RentalListing
{
    public int Id { get; set; }

    public MotorcycleCategory? Category { get; set; }

    [Required]
    [StringLength(60)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string City { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "10000")]
    public decimal DailyPrice { get; set; }

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public string OwnerId { get; set; } = null!;

    public IdentityUser Owner { get; set; } = null!;
}