using Golyath.Application.DTOs;

namespace Golyath.Application.Services;

public interface IPersonalRecordService
{
    /// <summary>
    /// Returns one <see cref="PersonalRecord"/> per exercise that has at least one completed set
    /// for the given user, ordered by exercise name.
    /// </summary>
    Task<IReadOnlyList<PersonalRecord>> GetPersonalRecordsAsync(int userId);
}
