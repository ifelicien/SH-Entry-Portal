using Microsoft.EntityFrameworkCore;
using SH_Entry_Portal.Data;
using SH_Entry_Portal.Models.Generated;

namespace SH_Entry_Portal.Services;

public class MemberService
{
    private readonly AppDbContext _context;

    public MemberService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Member>> GetMembersAsync()
    {
        return await _context.Members.ToListAsync();
    }

    public async Task AddMemberAsync(Member m, string changedBy)
    {
        _context.Members.Add(m);
        await _context.SaveChangesAsync();
        await LogAuditAsync(m.Id, "Created", changedBy);
    }

    // Persists in-place edits made to a tracked Member (status changes, inline edits) and logs who made them
    public async Task SaveChangesAsync(Guid memberId, string action, string changedBy)
    {
        await _context.SaveChangesAsync();
        await LogAuditAsync(memberId, action, changedBy);
    }

    private async Task LogAuditAsync(Guid memberId, string action, string changedBy)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            MemberId = memberId,
            Action = action,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}