namespace Golyath.Application.Interfaces;

public interface IDatabaseMigrationRunner
{
    Task MigrateAsync();
}
