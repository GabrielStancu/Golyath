using SQLite;

namespace Golyath.Infrastructure.Database.Migrations;

/// <summary>Tracks which schema migrations have been applied.</summary>
[Table("__SchemaVersions")]
internal class SchemaVersion
{
    [PrimaryKey]
    public int Version { get; set; }

    public DateTime AppliedAt { get; set; }
}
