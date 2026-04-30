using Golyath.Core.Entities;
using Golyath.Core.Enums;

namespace Golyath.Core.Abstractions;

public interface IExerciseRepository : IRepository<Exercise>
{
    Task<IReadOnlyList<Exercise>> SearchAsync(string query);
    Task<IReadOnlyList<Exercise>> GetByMuscleGroupAsync(MuscleGroup muscleGroup);
    Task<IReadOnlyList<Exercise>> GetByEquipmentAsync(EquipmentType equipment);
}
