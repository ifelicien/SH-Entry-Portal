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

    public List<Member> GetMembers()
    {
        return _context.Members.ToList();
    }

    public void AddMember(Member m)
    {
        _context.Members.Add(m);
        _context.SaveChanges();
    }

    // Persists in-place edits made to a tracked Member (status changes, inline edits)
    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}