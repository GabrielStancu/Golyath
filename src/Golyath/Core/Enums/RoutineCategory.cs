namespace Golyath.Core.Enums;

public enum RoutineCategory
{
    Push,
    Pull,
    Legs,
    Upper,
    Lower,
    FullBody,
    Core,
    Cardio,
    Custom
}

public static class RoutineCategoryExtensions
{
    public static string DisplayName(this RoutineCategory category) => category switch
    {
        RoutineCategory.FullBody => "Full Body",
        _ => category.ToString()
    };

    public static string HexColor(this RoutineCategory category) => category switch
    {
        RoutineCategory.Push => "#FFD700",
        RoutineCategory.Pull => "#4CAF50",
        RoutineCategory.Legs => "#FF7043",
        RoutineCategory.Upper => "#42A5F5",
        RoutineCategory.Lower => "#AB47BC",
        RoutineCategory.FullBody => "#26A69A",
        RoutineCategory.Core => "#EC407A",
        RoutineCategory.Cardio => "#EF5350",
        RoutineCategory.Custom => "#888888",
        _ => "#888888"
    };
}
