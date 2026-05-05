namespace Golyath.Core.Abstractions;

public interface IExerciseSeederService
{
    /// <summary>
    /// Seeds the exercise library from bundled JSON assets if it has not been seeded yet.
    /// Safe to call on every startup — exits immediately when exercises already exist.
    /// </summary>
    Task SeedAsync();
}
