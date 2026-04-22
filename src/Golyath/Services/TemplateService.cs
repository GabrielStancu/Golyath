using Golyath.Data;
using Golyath.Models;

namespace Golyath.Services;

public class TemplateService : ITemplateService
{
    private readonly GolyathDatabase _db;

    public TemplateService(GolyathDatabase db) => _db = db;

    public Task<List<WorkoutTemplate>> GetTemplatesAsync() =>
        _db.GetAllAsync<WorkoutTemplate>();

    public Task<WorkoutTemplate?> GetTemplateByIdAsync(int id) =>
        _db.GetByIdAsync<WorkoutTemplate>(id);

    public async Task<WorkoutTemplate> CreateTemplateAsync(WorkoutTemplate template)
    {
        template.CreatedAt = DateTime.UtcNow;
        await _db.InsertAsync(template);
        return template;
    }

    public Task UpdateTemplateAsync(WorkoutTemplate template) =>
        _db.UpdateAsync(template);

    public async Task DeleteTemplateAsync(int templateId)
    {
        var entries = await _db.QueryAsync<WorkoutTemplateEntry>(
            "SELECT * FROM WorkoutTemplateEntry WHERE TemplateId = ?", templateId);
        foreach (var e in entries)
            await _db.DeleteAsync(e);

        var template = await _db.GetByIdAsync<WorkoutTemplate>(templateId);
        if (template is not null)
            await _db.DeleteAsync(template);
    }

    public async Task<List<TemplateEntryDetail>> GetEntryDetailsAsync(int templateId)
    {
        var entries = await _db.QueryAsync<WorkoutTemplateEntry>(
            "SELECT * FROM WorkoutTemplateEntry WHERE TemplateId = ? ORDER BY SortOrder ASC",
            templateId);

        var result = new List<TemplateEntryDetail>();
        foreach (var entry in entries)
        {
            var exercise = await _db.GetByIdAsync<Exercise>(entry.ExerciseId);
            if (exercise is not null)
                result.Add(new TemplateEntryDetail(entry, exercise));
        }
        return result;
    }

    /// <summary>
    /// Replaces all entries for the given template with the supplied list.
    /// Existing entries are deleted and new ones inserted (preserving SortOrder).
    /// </summary>
    public async Task SaveEntriesAsync(int templateId, IEnumerable<WorkoutTemplateEntry> entries)
    {
        var existing = await _db.QueryAsync<WorkoutTemplateEntry>(
            "SELECT * FROM WorkoutTemplateEntry WHERE TemplateId = ?", templateId);
        foreach (var e in existing)
            await _db.DeleteAsync(e);

        int order = 0;
        foreach (var entry in entries)
        {
            entry.Id = 0; // force insert
            entry.TemplateId = templateId;
            entry.SortOrder = order++;
            await _db.InsertAsync(entry);
        }
    }
}
