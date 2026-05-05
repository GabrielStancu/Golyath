using Golyath.UI.ViewModels.Exercises;

namespace Golyath.UI.Views.Exercises;

public partial class CreateExercisePage : ContentPage
{
    public CreateExercisePage(CreateExerciseViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
