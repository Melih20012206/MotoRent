using System.ComponentModel.DataAnnotations;

namespace MotoRent.Web.Models;

public class CreateRentalRequestViewModel
{
    [Required(ErrorMessage = "Избери начална дата.")]
    [DataType(DataType.Date)]
    [Display(Name = "Начална дата")]
    public DateOnly? StartDate { get; set; }

    [Required(ErrorMessage = "Избери крайна дата.")]
    [DataType(DataType.Date)]
    [Display(Name = "Крайна дата")]
    public DateOnly? EndDate { get; set; }
}