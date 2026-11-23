using System.ComponentModel.DataAnnotations;

namespace MotoRent.Web.Models;

public enum MotorcycleCategory
{
    [Display(Name = "Пистов / Спортен")]
    Sport = 1,

    [Display(Name = "Нейкед")]
    Naked = 2,

    [Display(Name = "Турър")]
    Touring = 3,

    [Display(Name = "Адвенчър")]
    Adventure = 4,

    [Display(Name = "Ендуро")]
    Enduro = 5,

    [Display(Name = "Круизър / Чопър")]
    Cruiser = 6,

    [Display(Name = "Скутер")]
    Scooter = 7,

    [Display(Name = "Друг")]
    Other = 8
}