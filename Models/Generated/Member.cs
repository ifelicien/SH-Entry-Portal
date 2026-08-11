using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SH_Entry_Portal.Models.Generated;

public partial class Member
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = null!;

    [Phone]
    public string? Phone { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    // Manually added: skipped by scaffold since Postgres enums require a CLR type
    public MemberRole Role { get; set; }

    public MemberStatus Status { get; set; }

    // Manually added: real joined_on column added to Supabase after initial scaffold
    public DateOnly JoinedOn { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
