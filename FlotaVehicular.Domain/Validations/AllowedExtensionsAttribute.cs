using System.ComponentModel.DataAnnotations;

namespace FlotaVehicular.Domain.Validations;

[AttributeUsage(AttributeTargets.Property)]
public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedExtensionsAttribute(params string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string fileName || string.IsNullOrWhiteSpace(fileName))
            return ValidationResult.Success;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (_extensions.Contains(extension))
            return ValidationResult.Success;

        return new ValidationResult(
            ErrorMessage ?? $"El archivo debe tener una de estas extensiones: {string.Join(", ", _extensions)}.",
            new[] { validationContext.MemberName! });
    }
}
