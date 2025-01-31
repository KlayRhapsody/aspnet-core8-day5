
namespace WebApi.Models.Dtos;

public class CourseCreate : IValidatableObject
{
    public int CourseId { get; set; }

    [Required(ErrorMessage = "The Title field is required.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "The Credits field is required.")]
    [Range(0, 5, ErrorMessage = "The Credits field must be between 0 and 5.")]
    public int Credits { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CourseId != 0)
        {
            yield return new ValidationResult("The CourseId field must required 0.", [nameof(CourseId)]);
        }

        yield return ValidationResult.Success!;
    }
}
