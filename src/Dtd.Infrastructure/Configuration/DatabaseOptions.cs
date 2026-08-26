using System.ComponentModel.DataAnnotations;

namespace Dtd.Infrastructure.Configuration;

/// <summary>Database connection options (PostgreSQL via Npgsql).</summary>
public sealed class DatabaseOptions
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    public bool AutoApplyMigrations { get; set; } = false;
}