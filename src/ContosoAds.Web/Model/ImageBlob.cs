using System.ComponentModel.DataAnnotations;

namespace ContosoAds.Web.Model;

public record ImageBlob([Required] Uri Uri, [Required] int AdId);