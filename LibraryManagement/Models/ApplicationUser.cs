using Microsoft.AspNetCore.Identity;

namespace LibraryManagement.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}