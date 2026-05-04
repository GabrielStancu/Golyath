using CommunityToolkit.Mvvm.Messaging.Messages;
using Golyath.Core.Entities;

namespace Golyath.UI.ViewModels.Workout;

/// <summary>Sent when the user picks an exercise from the ExercisePickerPage.</summary>
public sealed class ExercisePickedMessage : ValueChangedMessage<Exercise>
{
    public ExercisePickedMessage(Exercise value) : base(value) { }
}
