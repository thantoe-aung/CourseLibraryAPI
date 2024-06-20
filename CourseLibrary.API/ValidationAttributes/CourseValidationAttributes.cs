using CourseLibrary.API.Models;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.ValidationAttributes
{
    public class CourseValidationAttributes : ValidationAttribute
    {
        public CourseValidationAttributes()
        {
            
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(validationContext.ObjectInstance is not CourseForManipulationDto course)
            {
                throw new Exception($"Some Exception Message");
            }

            if(course.Title == course.Description)
            {
                return new ValidationResult("message", new[] {nameof(CourseForManipulationDto)});
            }

            return ValidationResult.Success;
            
        }
    }
}
