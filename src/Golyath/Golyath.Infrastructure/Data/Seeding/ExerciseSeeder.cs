using System.Text.Json;
using System.Text.Json.Serialization;
using Golyath.Infrastructure.Data;
using Golyath.Infrastructure.Data.Models;

namespace Golyath.Infrastructure.Data.Seeding;

public class ExerciseSeeder
{
    private readonly AppDatabase _database;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ExerciseSeeder(AppDatabase database)
    {
        _database = database;
    }

    public async Task SeedAsync()
    {
        var count = await _database.Connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Exercises WHERE IsCustom = 0");

        if (count > 0)
            return;

        var assembly = typeof(ExerciseSeeder).Assembly;
        const string resourceName = "Golyath.Infrastructure.Data.Seeding.exercises-seed.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return;

        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var dtos = JsonSerializer.Deserialize<List<ExerciseSeedDto>>(json, _jsonOptions);
        if (dtos is null || dtos.Count == 0)
            return;

        var models = dtos.Select(dto => new ExerciseDbModel
        {
            Id = dto.Id ?? SanitizeId(dto.Name ?? string.Empty),
            Name = dto.Name ?? string.Empty,
            Force = dto.Force,
            Level = dto.Level,
            Mechanic = dto.Mechanic,
            Equipment = dto.Equipment,
            PrimaryMusclesJson = JsonSerializer.Serialize(dto.PrimaryMuscles ?? []),
            SecondaryMusclesJson = JsonSerializer.Serialize(dto.SecondaryMuscles ?? []),
            InstructionsJson = JsonSerializer.Serialize(dto.Instructions ?? []),
            Category = dto.Category,
            IsCustom = false,
            CreatedAt = DateTime.UtcNow.ToString("o"),
        }).ToList();

        await _database.Connection.InsertAllAsync(models);
    }

    private static string SanitizeId(string name) =>
        name.Replace(" ", "_").Replace("-", "_").Replace("/", "_");

    private class ExerciseSeedDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Force { get; set; }
        public string? Level { get; set; }
        public string? Mechanic { get; set; }
        public string? Equipment { get; set; }
        public List<string>? PrimaryMuscles { get; set; }
        public List<string>? SecondaryMuscles { get; set; }
        public List<string>? Instructions { get; set; }
        public string? Category { get; set; }
        [JsonPropertyName("images")]
        public List<string>? Images { get; set; }
    }
}
