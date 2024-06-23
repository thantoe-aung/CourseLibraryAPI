using System.Reflection;

namespace CourseLibrary.API.Services
{
    public class PropertyCheckerService : IPropertyCheckerService
    {
        public bool CheckPropertyExist<T>(string? fields)
        {
            if (fields == null)
            {
                return true;
            }

            var fieldsSeparate = fields.Split(',');
            foreach (var field in fieldsSeparate)
            {
                var propName = field.Trim();
                var propInfo = typeof(T).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propInfo == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
