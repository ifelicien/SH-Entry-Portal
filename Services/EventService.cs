using Microsoft.EntityFrameworkCore;
using SH_Entry_Portal.Data;
using SH_Entry_Portal.Models.Generated;

namespace SH_Entry_Portal.Services;

public class EventService
{
    private readonly AppDbContext _context;

    public EventService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Event>> GetEventsAsync()
    {
        return await _context.Events.OrderBy(e => e.EventTime).ToListAsync();
    }

    public async Task AddEventAsync(Event e, string changedBy)
    {
        _context.Events.Add(e);
        await _context.SaveChangesAsync();
        await LogAuditAsync(e.Id, "Created", changedBy);
    }

    // Persists in-place edits made to a tracked Event and logs who made them
    public async Task SaveChangesAsync(Guid eventId, string action, string changedBy)
    {
        await _context.SaveChangesAsync();
        await LogAuditAsync(eventId, action, changedBy);
    }

    public async Task DeleteEventAsync(Event e, string changedBy)
    {
        _context.Events.Remove(e);
        await _context.SaveChangesAsync();
        await LogAuditAsync(e.Id, "Deleted", changedBy);
    }

    private async Task LogAuditAsync(Guid eventId, string action, string changedBy)
    {
        _context.EventAuditLogs.Add(new EventAuditLog
        {
            EventId = eventId,
            Action = action,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
