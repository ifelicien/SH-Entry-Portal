using SH_Entry_Portal.Models;

namespace SH_Entry_Portal.Services;

public class MemberService
{
    private List<Member> _members = new();

    public List<Member> GetMembers()
    {
        return _members;
    }
    public void AddMember(Member m)
    {
        _members.Add(m);
    }
    
}