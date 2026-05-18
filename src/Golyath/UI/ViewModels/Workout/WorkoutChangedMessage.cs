using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Golyath.UI.ViewModels.Workout;

/// <summary>Broadcast when a workout is created, completed, or deleted.</summary>
public sealed class WorkoutChangedMessage : ValueChangedMessage<int>
{
    public WorkoutChangedMessage(int workoutId) : base(workoutId) { }
}
