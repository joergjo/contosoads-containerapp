using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ContosoAds.Web.Model;

public class Ad
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Currency)]
    public int Price { get; set; }

    [StringLength(1000)]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [StringLength(1000)]
    [DisplayName("Full-size image")]
    public string? ImageUri { get; set; }

    [StringLength(1000)]
    [DisplayName("Image")]
    public string? ThumbnailUri { get; set; }

    [DataType(DataType.Date)]
    [DisplayName("Posted on")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; }

    [Required]
    [Phone]
    [DisplayName("Phone number")]
    public string Phone { get; set; } = string.Empty;
    
    public bool IsNew => Id == 0;
}