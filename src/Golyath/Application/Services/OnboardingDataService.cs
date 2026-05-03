using Golyath.Core.Enums;

namespace Golyath.Application.Services;

/// <summary>Holds in-progress onboarding data as the user walks through the wizard.</summary>
public sealed class OnboardingDataService
{
    public string Nickname { get; set; } = string.Empty;
    public DateTime Birthday { get; set; } = DateTime.Today.AddYears(-25);
    public double HeightCm { get; set; } = 170;
    public double WeightKg { get; set; } = 70;
    public Gender Gender { get; set; } = Gender.PreferNotToSay;
    public FitnessGoal FitnessGoal { get; set; } = FitnessGoal.Balanced;
    public WeightUnit PreferredUnit { get; set; } = WeightUnit.Kg;
}
