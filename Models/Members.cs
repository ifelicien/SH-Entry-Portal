using System.ComponentModel.DataAnnotations;

namespace SH_Entry_Portal.Models;

public class Member
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Role { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Status { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? Phone { get; set; }

    public DateTime JoinedOn { get; set; } = DateTime.UtcNow;
}