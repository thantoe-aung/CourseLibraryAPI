using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers
{
    public static class IEenumerableExtension
    {
        public static IEnumerable<ExpandoObject> ShapeData<T>(this IEnumerable<T> source, string? fields)
        {
            if(source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var resultList = new List<ExpandoObject>();
            var propertyList = new List<PropertyInfo>();

            if (string.IsNullOrEmpty(fields))
            {
                var infos = typeof(T).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                propertyList.AddRange(infos);
            }
            else
            {
                var fieldList = fields.Split(",");
                foreach ( var field in fieldList)
                {
                    var propName = field.Trim();
                    var propInfo = typeof(T).GetProperty(propName,BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    
                    if(propInfo == null)
                    {
                        throw new Exception($"Property Not Found");
                    }

                    propertyList.Add(propInfo);
                }
            }

            foreach(var item in source)
            {
                var dataShapeObject = new ExpandoObject();
                foreach(var property in propertyList)
                {
                    var value = property.GetValue(item);

                    ((IDictionary<string,object?>)dataShapeObject).Add(property.Name, value);
                }
                resultList.Add(dataShapeObject);
            }

            return resultList;
        }
    }
}
