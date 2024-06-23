namespace CourseLibrary.API.Services
{
    public interface IPropertyCheckerService
    {
        bool CheckPropertyExist<T>(string? fields);
    }
}