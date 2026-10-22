using System.ComponentModel.DataAnnotations;

namespace UniversityApi.Common.Validation;

/// <summary>
/// Проверяет, что дата рождения не в будущем и возраст попадает в допустимый диапазон.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class BirthDateAttribute : ValidationAttribute
{
    public int MinAge { get; set; } = 14;
    public int MaxAge { get; set; } = 100;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success; // обязательность проверяет [Required]

        if (value is not DateOnly birthDate)
            return new ValidationResult("BirthDate has an invalid format");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (birthDate > today)
            return new ValidationResult("BirthDate cannot be in the future", new[] { validationContext.MemberName! });

        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age))
            age--;

        if (age < MinAge || age > MaxAge)
            return new ValidationResult(
                $"Student age must be between {MinAge} and {MaxAge} years",
                new[] { validationContext.MemberName! });

        return ValidationResult.Success;
    }
}
