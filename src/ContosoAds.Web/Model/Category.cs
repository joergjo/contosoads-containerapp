using System.ComponentModel.DataAnnotations;

namespace ContosoAds.Web.Model;

public enum Category
{
    Cars,
    [Display(Name = "Real Estate")]
    RealEstate,
    [Display(Name = "Free Stuff")]
    FreeStuff
}