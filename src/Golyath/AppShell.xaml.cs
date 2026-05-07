using Golyath.UI.Views.Exercises;
using Golyath.UI.Views.History;
using Golyath.UI.Views.Profile;
using Golyath.UI.Views.Workout;

namespace Golyath;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(EditProfilePage), typeof(EditProfilePage));
        Routing.RegisterRoute(nameof(ActiveWorkoutPage), typeof(ActiveWorkoutPage));
        Routing.RegisterRoute(nameof(ExercisePickerPage), typeof(ExercisePickerPage));
        Routing.RegisterRoute(nameof(ExerciseDetailPage), typeof(ExerciseDetailPage));
        Routing.RegisterRoute(nameof(CreateExercisePage), typeof(CreateExercisePage));
        Routing.RegisterRoute("WorkoutDetail", typeof(WorkoutDetailPage));
    }
}
