using System.ComponentModel.DataAnnotations;

namespace English_games.Validation;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSize;

    public MaxFileSizeAttribute(int maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            if (file.Length > _maxFileSize)
            {
                return new ValidationResult($"Максимальный размер файла - {_maxFileSize / (1024 * 1024)} МБ");
            }
        }

        return ValidationResult.Success;
    }
}