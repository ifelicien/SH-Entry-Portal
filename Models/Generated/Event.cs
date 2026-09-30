using System;
using System.ComponentModel.DataAnnotations;

namespace SH_Entry_Portal.Models.Generated;

// Manually added: second table alongside Members, for tracking sisterhood events
public class Event
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public DateTime EventTime { get; set; }

    public bool IsPaid { get; set; }

    // Only meaningful when IsPaid is true
    public decimal? Price { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
