namespace StrongHub.Models;

/// <summary>Calculates the body mass index.</summary>
public static class BmiCalculator
{
    /// <summary>BMI from weight in kilograms and height in meters.</summary>
    public static double Calculate(double weightKg, double heightM)
    {
        if (weightKg <= 0 || heightM <= 0)
        {
            throw new ValidationException("Weight and height must be greater than zero.");
        }

        return Math.Round(weightKg / (heightM * heightM), 2);
    }
}
