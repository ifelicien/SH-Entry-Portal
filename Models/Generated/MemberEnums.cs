using NpgsqlTypes;

namespace SH_Entry_Portal.Models.Generated;

// Members must match the Postgres enum labels exactly (see NpgsqlNullNameTranslator usage in AppDbContext).
// [PgName] is needed only where the label contains a space, since that's not a valid C# identifier.
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
