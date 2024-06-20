using CourseLibrary.API.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    [CourseValidationAttributes]
    public abstract class CourseForManipulationDto // : IValidatableObject
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1500),]
        public virtual string Description { get; set; } = string.Empty;

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if(Title == Description)
        //    {
        //        yield return new ValidationResult("Title shoud diff from desc", new[] {"Course"});
        //    }
        //}
    }
}
