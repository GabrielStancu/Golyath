using System.Text.Json;
using Golyath.Application.DTOs;
using Golyath.Core.Abstractions;
using Golyath.Core.Entities;

namespace Golyath.Application.Services;

public sealed class DataPortabilityService : IDataPortabilityService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Required for deserializing records: constructor parameter names are matched
        // against JSON property names case-insensitively so 'id' matches 'Id' etc.
        PropertyNameCaseInsensitive = true
    };

    private readonly IUserRepository _userRepo;
    private readonly IExerciseRepository _exerciseRepo;
    private readonly IWorkoutRepository _workoutRepo;
    private readonly IWorkoutExerciseRepository _workoutExerciseRepo;
    private readonly IWorkoutSetRepository _workoutSetRepo;
    private readonly IGoalRepository _goalRepo;
    private readonly ITagRepository _tagRepo;
    private readonly IWorkoutTagRepository _workoutTagRepo;

    public DataPortabilityService(
        IUserRepository userRepo,
        IExerciseRepository exerciseRepo,
        IWorkoutRepository workoutRepo,
        IWorkoutExerciseRepository workoutExerciseRepo,
        IWorkoutSetRepository workoutSetRepo,
        IGoalRepository goalRepo,
        ITagRepository tagRepo,
        IWorkoutTagRepository workoutTagRepo)
    {
        _userRepo = userRepo;
        _exerciseRepo = exerciseRepo;
        _workoutRepo = workoutRepo;
        _workoutExerciseRepo = workoutExerciseRepo;
        _workoutSetRepo = workoutSetRepo;
        _goalRepo = goalRepo;
        _tagRepo = tagRepo;
        _workoutTagRepo = workoutTagRepo;
    }

    public async Task<string> ExportToJsonAsync()
    {
        var usersTask = _userRepo.GetAllAsync();
        var exercisesTask = _exerciseRepo.GetAllAsync();
        var workoutsTask = _workoutRepo.GetAllAsync();
        var workoutExercisesTask = _workoutExerciseRepo.GetAllAsync();
        var workoutSetsTask = _workoutSetRepo.GetAllAsync();
        var goalsTask = _goalRepo.GetAllAsync();
        var tagsTask = _tagRepo.GetAllAsync();
        var workoutTagsTask = _workoutTagRepo.GetAllAsync();

        await Task.WhenAll(usersTask, exercisesTask, workoutsTask, workoutExercisesTask,
            workoutSetsTask, goalsTask, tagsTask, workoutTagsTask);

        var doc = new BackupDocument
        {
            Data = new BackupData
            {
                Users = usersTask.Result.Select(u => new UserBackup(
                    u.Id, u.Nickname, u.Birthday, u.HeightCm, u.WeightKg,
                    u.Gender, u.FitnessGoal, u.PreferredUnit, u.CreatedAt, u.UpdatedAt)).ToList(),

                Exercises = exercisesTask.Result.Select(e => new ExerciseBackup(
                    e.Id, e.Name, e.IsCustom, e.PrimaryMuscle, e.SecondaryMusclesRaw,
                    e.MovementType, e.Equipment, e.Notes, e.ExternalId, e.InstructionsRaw)).ToList(),

                Workouts = workoutsTask.Result.Select(w => new WorkoutBackup(
                    w.Id, w.Name, w.StartedAt, w.CompletedAt, w.DurationSeconds,
                    w.Notes, w.CreatedAt)).ToList(),

                WorkoutExercises = workoutExercisesTask.Result.Select(we => new WorkoutExerciseBackup(
                    we.Id, we.WorkoutId, we.ExerciseId, we.Order, we.Notes)).ToList(),

                WorkoutSets = workoutSetsTask.Result.Select(ws => new WorkoutSetBackup(
                    ws.Id, ws.WorkoutExerciseId, ws.SetNumber, ws.Weight, ws.Reps,
                    ws.Tempo, ws.Notes, ws.IsCompleted, ws.CompletedAt)).ToList(),

                Goals = goalsTask.Result.Select(g => new GoalBackup(
                    g.Id, g.UserId, g.Type, g.Description, g.TargetValue, g.CurrentValue,
                    g.ExerciseId, g.TargetDate, g.IsCompleted, g.CreatedAt, g.UpdatedAt)).ToList(),

                Tags = tagsTask.Result.Select(t => new TagBackup(t.Id, t.Name, t.Color)).ToList(),

                WorkoutTags = workoutTagsTask.Result.Select(wt => new WorkoutTagBackup(
                    wt.Id, wt.WorkoutId, wt.TagId)).ToList()
            }
        };

        return JsonSerializer.Serialize(doc, JsonOptions);
    }

    public async Task<ImportResult> ImportFromStreamAsync(Stream stream)
    {
        // Read raw bytes — avoids any encoding transformation that StreamReader or
        // content:// provider wrappers might silently apply.
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();

        // Strip UTF-8 BOM (EF BB BF) at byte level before converting to string.
        int start = (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) ? 3 : 0;
        var json = System.Text.Encoding.UTF8.GetString(bytes, start, bytes.Length - start).Trim();

        return await ImportFromJsonAsync(json);
    }

    public async Task<ImportResult> ImportFromJsonAsync(string json)
    {
        try
        {
            json = json.TrimStart('\uFEFF').Trim();

            var doc = JsonSerializer.Deserialize<BackupDocument>(json, JsonOptions);
            if (doc is null)
                return new ImportResult(false, "Failed to parse backup file.");

            if (doc.BackupSchemaVersion != 1)
                return new ImportResult(false,
                    $"Incompatible backup version: {doc.BackupSchemaVersion}. This app supports version 1.");

            var data = doc.Data;
            int itemsImported = 0;

            // Load all existing data into memory to avoid N+1 queries
            var existingUsersTask = _userRepo.GetAllAsync();
            var existingExercisesTask = _exerciseRepo.GetAllAsync();
            var existingWorkoutsTask = _workoutRepo.GetAllAsync();
            var existingWorkoutExercisesTask = _workoutExerciseRepo.GetAllAsync();
            var existingWorkoutSetsTask = _workoutSetRepo.GetAllAsync();
            var existingGoalsTask = _goalRepo.GetAllAsync();
            var existingTagsTask = _tagRepo.GetAllAsync();

            await Task.WhenAll(existingUsersTask, existingExercisesTask, existingWorkoutsTask,
                existingWorkoutExercisesTask, existingWorkoutSetsTask, existingGoalsTask, existingTagsTask);

            var existingUsers = existingUsersTask.Result;
            var existingExercises = existingExercisesTask.Result;
            var existingWorkouts = existingWorkoutsTask.Result;
            var existingWorkoutExercises = existingWorkoutExercisesTask.Result;
            var existingWorkoutSets = existingWorkoutSetsTask.Result;
            var existingGoals = existingGoalsTask.Result;
            var existingTags = existingTagsTask.Result;

            // ID maps: backupId → localId
            var tagIdMap = new Dictionary<int, int>();
            var userIdMap = new Dictionary<int, int>();
            var exerciseIdMap = new Dictionary<int, int>();
            var workoutIdMap = new Dictionary<int, int>();
            var workoutExerciseIdMap = new Dictionary<int, int>();

            // a) Tags — dedup by Name
            foreach (var backupTag in data.Tags)
            {
                var existing = existingTags.FirstOrDefault(t =>
                    string.Equals(t.Name, backupTag.Name, StringComparison.OrdinalIgnoreCase));

                if (existing is not null)
                {
                    tagIdMap[backupTag.Id] = existing.Id;
                }
                else
                {
                    var newTag = new Tag { Name = backupTag.Name, Color = backupTag.Color };
                    int newId = await _tagRepo.InsertAsync(newTag);
                    tagIdMap[backupTag.Id] = newId;
                    itemsImported++;
                }
            }

            // b) Users — dedup by Nickname
            foreach (var backupUser in data.Users)
            {
                var existing = existingUsers.FirstOrDefault(u =>
                    string.Equals(u.Nickname, backupUser.Nickname, StringComparison.OrdinalIgnoreCase));

                if (existing is not null)
                {
                    userIdMap[backupUser.Id] = existing.Id;
                }
                else
                {
                    var newUser = new User
                    {
                        Nickname = backupUser.Nickname,
                        Birthday = backupUser.Birthday,
                        HeightCm = backupUser.HeightCm,
                        WeightKg = backupUser.WeightKg,
                        Gender = backupUser.Gender,
                        FitnessGoal = backupUser.FitnessGoal,
                        PreferredUnit = backupUser.PreferredUnit,
                        CreatedAt = backupUser.CreatedAt,
                        UpdatedAt = backupUser.UpdatedAt
                    };
                    int newId = await _userRepo.InsertAsync(newUser);
                    userIdMap[backupUser.Id] = newId;
                    itemsImported++;
                }
            }

            // c) Exercises — dedup strategy depends on IsCustom
            foreach (var backupExercise in data.Exercises)
            {
                if (!backupExercise.IsCustom)
                {
                    // Seeded: find by ExternalId
                    var existing = existingExercises.FirstOrDefault(e =>
                        !e.IsCustom && e.ExternalId == backupExercise.ExternalId);

                    if (existing is not null)
                        exerciseIdMap[backupExercise.Id] = existing.Id;
                    // else: skip — seeded exercise not present locally yet, don't insert
                }
                else
                {
                    // Custom: find by IsCustom && Name
                    var existing = existingExercises.FirstOrDefault(e =>
                        e.IsCustom &&
                        string.Equals(e.Name, backupExercise.Name, StringComparison.OrdinalIgnoreCase));

                    if (existing is not null)
                    {
                        exerciseIdMap[backupExercise.Id] = existing.Id;
                    }
                    else
                    {
                        var newExercise = new Exercise
                        {
                            Name = backupExercise.Name,
                            IsCustom = true,
                            PrimaryMuscle = backupExercise.PrimaryMuscle,
                            SecondaryMusclesRaw = backupExercise.SecondaryMusclesRaw,
                            MovementType = backupExercise.MovementType,
                            Equipment = backupExercise.Equipment,
                            Notes = backupExercise.Notes,
                            ExternalId = backupExercise.ExternalId,
                            InstructionsRaw = backupExercise.InstructionsRaw
                        };
                        int newId = await _exerciseRepo.InsertAsync(newExercise);
                        exerciseIdMap[backupExercise.Id] = newId;
                        itemsImported++;
                    }
                }
            }

            // d) Workouts — dedup by StartedAt (UTC exact match)
            foreach (var backupWorkout in data.Workouts)
            {
                var existing = existingWorkouts.FirstOrDefault(w =>
                    w.StartedAt == backupWorkout.StartedAt);

                if (existing is not null)
                {
                    workoutIdMap[backupWorkout.Id] = existing.Id;
                }
                else
                {
                    var newWorkout = new Workout
                    {
                        Name = backupWorkout.Name,
                        StartedAt = backupWorkout.StartedAt,
                        CompletedAt = backupWorkout.CompletedAt,
                        DurationSeconds = backupWorkout.DurationSeconds,
                        Notes = backupWorkout.Notes,
                        CreatedAt = backupWorkout.CreatedAt
                    };
                    int newId = await _workoutRepo.InsertAsync(newWorkout);
                    workoutIdMap[backupWorkout.Id] = newId;
                    itemsImported++;
                }
            }

            // e) Goals — dedup by (remapped UserId + Type + Description)
            foreach (var backupGoal in data.Goals)
            {
                if (!userIdMap.TryGetValue(backupGoal.UserId, out int localUserId))
                    continue;

                int? localExerciseId = null;
                if (backupGoal.ExerciseId.HasValue)
                {
                    if (exerciseIdMap.TryGetValue(backupGoal.ExerciseId.Value, out int mappedExId))
                        localExerciseId = mappedExId;
                    else
                        localExerciseId = null;
                }

                var existing = existingGoals.FirstOrDefault(g =>
                    g.UserId == localUserId &&
                    g.Type == backupGoal.Type &&
                    string.Equals(g.Description, backupGoal.Description, StringComparison.Ordinal));

                if (existing is not null)
                    continue;

                var newGoal = new Goal
                {
                    UserId = localUserId,
                    Type = backupGoal.Type,
                    Description = backupGoal.Description,
                    TargetValue = backupGoal.TargetValue,
                    CurrentValue = backupGoal.CurrentValue,
                    ExerciseId = localExerciseId,
                    TargetDate = backupGoal.TargetDate,
                    IsCompleted = backupGoal.IsCompleted,
                    CreatedAt = backupGoal.CreatedAt,
                    UpdatedAt = backupGoal.UpdatedAt
                };
                await _goalRepo.InsertAsync(newGoal);
                itemsImported++;
            }

            // f) WorkoutExercises — dedup by (remapped WorkoutId + remapped ExerciseId + Order)
            foreach (var backupWe in data.WorkoutExercises)
            {
                if (!workoutIdMap.TryGetValue(backupWe.WorkoutId, out int localWorkoutId))
                    continue;

                if (!exerciseIdMap.TryGetValue(backupWe.ExerciseId, out int localExerciseId))
                    continue;

                var existing = existingWorkoutExercises.FirstOrDefault(we =>
                    we.WorkoutId == localWorkoutId &&
                    we.ExerciseId == localExerciseId &&
                    we.Order == backupWe.Order);

                if (existing is not null)
                {
                    workoutExerciseIdMap[backupWe.Id] = existing.Id;
                }
                else
                {
                    var newWe = new WorkoutExercise
                    {
                        WorkoutId = localWorkoutId,
                        ExerciseId = localExerciseId,
                        Order = backupWe.Order,
                        Notes = backupWe.Notes
                    };
                    int newId = await _workoutExerciseRepo.InsertAsync(newWe);
                    workoutExerciseIdMap[backupWe.Id] = newId;
                    itemsImported++;
                }
            }

            // g) WorkoutSets — dedup by (remapped WorkoutExerciseId + SetNumber)
            foreach (var backupSet in data.WorkoutSets)
            {
                if (!workoutExerciseIdMap.TryGetValue(backupSet.WorkoutExerciseId, out int localWeId))
                    continue;

                var existing = existingWorkoutSets.FirstOrDefault(ws =>
                    ws.WorkoutExerciseId == localWeId &&
                    ws.SetNumber == backupSet.SetNumber);

                if (existing is not null)
                    continue;

                var newSet = new WorkoutSet
                {
                    WorkoutExerciseId = localWeId,
                    SetNumber = backupSet.SetNumber,
                    Weight = backupSet.Weight,
                    Reps = backupSet.Reps,
                    Tempo = backupSet.Tempo,
                    Notes = backupSet.Notes,
                    IsCompleted = backupSet.IsCompleted,
                    CompletedAt = backupSet.CompletedAt
                };
                await _workoutSetRepo.InsertAsync(newSet);
                itemsImported++;
            }

            // h) WorkoutTags — AddAsync is already idempotent
            foreach (var backupWt in data.WorkoutTags)
            {
                if (!workoutIdMap.TryGetValue(backupWt.WorkoutId, out int localWorkoutId))
                    continue;

                if (!tagIdMap.TryGetValue(backupWt.TagId, out int localTagId))
                    continue;

                await _workoutTagRepo.AddAsync(localWorkoutId, localTagId);
            }

            return new ImportResult(true, "Import complete.", itemsImported);
        }
        catch (JsonException)
        {
            var preview = json.Length >= 10 ? json[..10] : json;
            return new ImportResult(false, $"The selected file is not a valid Golyath backup (starts with: '{preview}').");
        }
        catch (Exception ex)
        {
            return new ImportResult(false, ex.Message);
        }
    }
}
