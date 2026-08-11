using NpgsqlTypes;

namespace SH_Entry_Portal.Models.Generated;

public enum MemberRole
{
    Member,
    Officer,
    President,
    [PgName("Vice President")]
    VicePresident,
    Secretary,
    Treasurer,
    Chaplain
}

public enum MemberStatus
{
    Active,
    Inactive,
    Pending,
    Honorary
}
