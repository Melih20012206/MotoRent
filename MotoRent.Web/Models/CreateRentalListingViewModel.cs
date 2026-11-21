using System.ComponentModel.DataAnnotations;

namespace MotoRent.Web.Models;

public class CreateRentalListingViewModel
{
    [Required]
    [StringLength(60)]
    [Display(Name = "Марка")]
    public string Make { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "Модел")]
    public string Model { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "Град")]
    public string City { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "1", "10000")]
    [Display(Name = "Цена на ден (€)")]
    public decimal? DailyPrice { get; set; }

    [Required]
    [StringLength(2000)]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;
}