using Golyath.UI.Views.Exercises;
using Golyath.UI.Views.Goals;
using Golyath.UI.Views.History;
using Golyath.UI.Views.Profile;
using Golyath.UI.Views.Settings;
using Golyath.UI.Views.Suggestions;
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
        Routing.RegisterRoute(nameof(RoutineBuilderPage), typeof(RoutineBuilderPage));
        Routing.RegisterRoute(nameof(ExerciseDetailPage), typeof(ExerciseDetailPage));
        Routing.RegisterRoute(nameof(CreateExercisePage), typeof(CreateExercisePage));
        Routing.RegisterRoute("WorkoutDetail", typeof(WorkoutDetailPage));
        Routing.RegisterRoute(nameof(AddGoalPage), typeof(AddGoalPage));
        Routing.RegisterRoute("GoalsPage", typeof(GoalsPage));
        Routing.RegisterRoute("SettingsPage", typeof(SettingsPage));
        Routing.RegisterRoute("SuggestionsPage", typeof(SuggestionsPage));
    }
}
