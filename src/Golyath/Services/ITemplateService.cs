using Golyath.Models;

namespace Golyath.Services;

public record TemplateEntryDetail(WorkoutTemplateEntry Entry, Exercise Exercise);

public interface ITemplateService
{
    Task<List<WorkoutTemplate>> GetTemplatesAsync();
    Task<WorkoutTemplate?> GetTemplateByIdAsync(int id);
    Task<WorkoutTemplate> CreateTemplateAsync(WorkoutTemplate template);
    Task UpdateTemplateAsync(WorkoutTemplate template);
    Task DeleteTemplateAsync(int templateId);

    Task<List<TemplateEntryDetail>> GetEntryDetailsAsync(int templateId);
    Task SaveEntriesAsync(int templateId, IEnumerable<WorkoutTemplateEntry> entries);
}
