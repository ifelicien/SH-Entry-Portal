using System;

namespace SH_Entry_Portal.Models.Generated;

// Manually added: tracks who changed a member record and when
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? MemberId { get; set; }
    public string Action { get; set; } = null!;
    public string ChangedBy { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
}
