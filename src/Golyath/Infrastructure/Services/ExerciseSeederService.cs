using System.Text.Json;
using System.Text.Json.Serialization;
using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Core.Enums;

namespace Golyath.Infrastructure.Services;

public sealed class ExerciseSeederService : IExerciseSeederService
{
    private readonly IExerciseRepository _repository;

    private static readonly Dictionary<string, MuscleGroup> MuscleGroupMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["abdominals"]    = MuscleGroup.Abs,
        ["abductors"]     = MuscleGroup.Glutes,
        ["adductors"]     = MuscleGroup.Quads,
        ["biceps"]        = MuscleGroup.Biceps,
        ["calves"]        = MuscleGroup.Calves,
        ["chest"]         = MuscleGroup.Chest,
        ["forearms"]      = MuscleGroup.Forearms,
        ["glutes"]        = MuscleGroup.Glutes,
        ["hamstrings"]    = MuscleGroup.Hamstrings,
        ["lats"]          = MuscleGroup.Back,
        ["lower back"]    = MuscleGroup.Back,
        ["middle back"]   = MuscleGroup.Back,
        ["neck"]          = MuscleGroup.FullBody,
        ["quadriceps"]    = MuscleGroup.Quads,
        ["shoulders"]     = MuscleGroup.Shoulders,
        ["traps"]         = MuscleGroup.Shoulders,
        ["triceps"]       = MuscleGroup.Triceps,
    };

    private static readonly Dictionary<string, EquipmentType> EquipmentMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["barbell"]       = EquipmentType.Barbell,
        ["cable"]         = EquipmentType.Cable,
        ["dumbbell"]      = EquipmentType.Dumbbell,
        ["e-z curl bar"]  = EquipmentType.Barbell,
        ["kettlebells"]   = EquipmentType.Kettlebell,
        ["bands"]         = EquipmentType.ResistanceBand,
        ["body only"]     = EquipmentType.Bodyweight,
        ["machine"]       = EquipmentType.Machine,
    };

    public ExerciseSeederService(IExerciseRepository repository)
    {
        _repository = repository;
    }

    public async Task SeedAsync()
    {
        var existing = await _repository.GetAllAsync();
        if (existing.Count > 0)
            return;

        var ids = await LoadIndexAsync();
        var exercises = new List<Exercise>(ids.Count);

        foreach (var id in ids)
        {
            var exercise = await LoadExerciseAsync(id);
            if (exercise is not null)
                exercises.Add(exercise);
        }

        foreach (var exercise in exercises)
            await _repository.InsertAsync(exercise);
    }

    private static async Task<List<string>> LoadIndexAsync()
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync("exercises_index.json");
        return await JsonSerializer.DeserializeAsync<List<string>>(stream) ?? [];
    }

    private static async Task<Exercise?> LoadExerciseAsync(string id)
    {
        try
        {
            var assetPath = $"exercises/{id}.json";
            await using var stream = await FileSystem.OpenAppPackageFileAsync(assetPath);
            var dto = await JsonSerializer.DeserializeAsync<ExerciseJsonDto>(stream, JsonOptions);
            if (dto is null) return null;

            var primaryMuscle = dto.PrimaryMuscles.Count > 0
                ? MapMuscle(dto.PrimaryMuscles[0])
                : MuscleGroup.FullBody;

            var secondaryMuscles = dto.SecondaryMuscles
                .Select(MapMuscle)
                .Distinct()
                .ToList();

            var exercise = new Exercise
            {
                Name = dto.Name,
                ExternalId = id,
                PrimaryMuscle = primaryMuscle,
                Equipment = MapEquipment(dto.Equipment),
                MovementType = DeriveMovementType(primaryMuscle),
                IsCustom = false,
            };

            exercise.SecondaryMuscles = secondaryMuscles;
            exercise.Instructions = dto.Instructions;

            return exercise;
        }
        catch
        {
            // Skip malformed or missing entries
            return null;
        }
    }

    private static MuscleGroup MapMuscle(string raw) =>
        MuscleGroupMap.TryGetValue(raw, out var group) ? group : MuscleGroup.FullBody;

    private static EquipmentType MapEquipment(string? raw) =>
        raw is not null && EquipmentMap.TryGetValue(raw, out var equipment)
            ? equipment
            : EquipmentType.Other;

    private static MovementType DeriveMovementType(MuscleGroup primary) => primary switch
    {
        MuscleGroup.Chest or MuscleGroup.Shoulders or MuscleGroup.Triceps => MovementType.Push,
        MuscleGroup.Back or MuscleGroup.Biceps or MuscleGroup.Forearms    => MovementType.Pull,
        MuscleGroup.Quads or MuscleGroup.Hamstrings
            or MuscleGroup.Glutes or MuscleGroup.Calves                   => MovementType.Legs,
        MuscleGroup.Abs                                                    => MovementType.Core,
        _                                                                  => MovementType.Other,
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // DTO for deserializing the open-source exercise JSON files
    private sealed class ExerciseJsonDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Equipment { get; set; }

        [JsonPropertyName("primaryMuscles")]
        public List<string> PrimaryMuscles { get; set; } = [];

        [JsonPropertyName("secondaryMuscles")]
        public List<string> SecondaryMuscles { get; set; } = [];

        public List<string> Instructions { get; set; } = [];
        public string Id { get; set; } = string.Empty;
    }
}
