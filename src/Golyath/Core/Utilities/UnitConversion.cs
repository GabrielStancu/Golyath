namespace Golyath.Core.Utilities;

public static class UnitConversion
{
    public static double CmToInches(double cm) => cm / 2.54;
    public static double InchesToCm(double inches) => inches * 2.54;
    public static double KgToLb(double kg) => kg * 2.20462;
    public static double LbToKg(double lb) => lb / 2.20462;
}
