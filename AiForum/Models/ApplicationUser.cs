using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ApplicationUser : IdentityUser
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string? ImageFilename { get; set; }

    [NotMapped]
    public IFormFile? ImageFile { get; set; } // Not mapped to the database
}
