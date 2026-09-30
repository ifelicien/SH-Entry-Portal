using System;

namespace SH_Entry_Portal.Models.Generated;

// Manually added: tracks who changed an event record and when
public class EventAuditLog
{
    public Guid Id { get; set; }
    public Guid? EventId { get; set; }
    public string Action { get; set; } = null!;
    public string ChangedBy { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
}
