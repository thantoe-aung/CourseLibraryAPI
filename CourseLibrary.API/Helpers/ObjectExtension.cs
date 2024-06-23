using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers
{
    public static class ObjectExtension
    {
        public static ExpandoObject ShapeData<T>(this T source, string? fields)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var result = new ExpandoObject();

            if (string.IsNullOrEmpty(fields))
            {
                var infos = typeof(T).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                foreach (var info in infos)
                {
                    var value = info.GetValue(source);
                    ((IDictionary<string, object?>)result).Add(info.Name, value);
                }

                return result;
            }

            var fieldList = fields.Split(",");

            foreach (var field in fieldList)
            {
                var propName = field.Trim();
                var propInfo = typeof(T).GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propInfo == null)
                {
                    throw new Exception($"Property Not Found");
                }

                var value = propInfo.GetValue(source);
                ((IDictionary<string, object?>)result).Add(propInfo.Name, value);

            }

            return result;
        }
    }
}
