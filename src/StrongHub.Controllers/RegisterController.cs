using System.Globalization;
using StrongHub.Data;
using StrongHub.Models;

namespace StrongHub.Controllers;

/// <summary>Validates and saves new basic users.</summary>
public class RegisterController
{
    private readonly IRepository<BasicUser> _users;
    private readonly IRepository<Trainer> _trainers;

    public RegisterController(IRepository<BasicUser> users, IRepository<Trainer> trainers)
    {
        _users = users;
        _trainers = trainers;
    }

    /// <summary>Registers a user from raw form text. Throws ValidationException if invalid.</summary>
    public BasicUser Register(string name, string username, string ageText, string weightText, string heightText)
    {
        name = name.Trim();
        username = username.Trim();

        ValidateName(name);
        ValidateUsername(username);
        int age = ParseAge(ageText);
        double weight = ParseNumber(weightText, "Weight", 20, 300);
        double height = ParseNumber(heightText, "Height", 1.0, 2.5);

        var user = new BasicUser
        {
            Name = name,
            Username = username,
            Age = age,
            WeightKg = weight,
            HeightM = height,
            Bmi = BmiCalculator.Calculate(weight, height)
        };

        _users.Add(user);
        return user;
    }

    /// <summary>BMI preview for the form, or null while the input is incomplete.</summary>
    public double? PreviewBmi(string weightText, string heightText)
    {
        if (!TryParseNumber(weightText, out double weight) || !TryParseNumber(heightText, out double height))
        {
            return null;
        }

        return weight > 0 && height > 0 ? BmiCalculator.Calculate(weight, height) : null;
    }

    private static void ValidateName(string name)
    {
        if (name.Length == 0)
        {
            throw new ValidationException("Name is required.");
        }
    }

    private void ValidateUsername(string username)
    {
        if (username.Length < 3 || username.Contains(' '))
        {
            throw new ValidationException("Username must have at least 3 characters and no spaces.");
        }

        if (IsUsernameTaken(username))
        {
            throw new ValidationException($"Username '{username}' is already in use.");
        }
    }

    private bool IsUsernameTaken(string username) =>
        _users.GetAll().Any(user => SameUsername(user.Username, username))
        || _trainers.GetAll().Any(trainer => SameUsername(trainer.Username, username));

    private static bool SameUsername(string first, string second) =>
        string.Equals(first, second, StringComparison.OrdinalIgnoreCase);

    private static int ParseAge(string text)
    {
        if (!int.TryParse(text.Trim(), out int age) || age < 10 || age > 100)
        {
            throw new ValidationException("Age must be a whole number between 10 and 100.");
        }

        return age;
    }

    private static double ParseNumber(string text, string field, double min, double max)
    {
        if (!TryParseNumber(text, out double value) || value < min || value > max)
        {
            throw new ValidationException($"{field} must be a number between {min} and {max}.");
        }

        return value;
    }

    private static bool TryParseNumber(string text, out double value) =>
        double.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Float,
            CultureInfo.InvariantCulture, out value);
}
