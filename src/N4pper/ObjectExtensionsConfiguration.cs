using OMnG;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace N4pper
{
    public class ObjectExtensionsConfiguration : OMnG.ObjectExtensionsConfiguration.DelegateILCachingConfiguration
    {
        private static Dictionary<string, Func<object, object>> _converters = new Dictionary<string, Func<object, object>>();

        protected override object ParseValue(PropertyInfo property, object target, object value)
        {
            if (value == null)
                return property.PropertyType.GetDefault();
            else
            {
                if (property.PropertyType.IsDateTime() && value.GetType().IsNumeric())
                {
                    DateTimeOffset d = DateTimeOffset.FromUnixTimeMilliseconds((long)Convert.ChangeType(value, typeof(long)));

                    if (property.PropertyType.IsAssignableFrom(typeof(DateTimeOffset)))
                        return d.ToLocalTime();
                    else
                        return d.ToLocalTime().DateTime;
                }
                else if (property.PropertyType.IsTimeSpan() && value.GetType().IsNumeric())
                {
                    return TimeSpan.FromMilliseconds((long)Convert.ChangeType(value, typeof(long)));
                }
                else if ((property.PropertyType == typeof(Guid) || property.PropertyType == typeof(Guid?)) && (value.GetType() == typeof(Guid) || value.GetType() == typeof(Guid?)))
                {
                    return value?.ToString();
                }
                else if ((property.PropertyType == typeof(Guid) || property.PropertyType == typeof(Guid?)) && value.GetType() == typeof(string))
                {
                    return Guid.Parse(value.ToString());
                }
                else if (value.GetType() != property.PropertyType)
                {
                    string convName = $"{value.GetType().FullName}{property.PropertyType.FullName}";

                    if (!_converters.ContainsKey(convName))
                    {
                        ParameterExpression p = Expression.Parameter(typeof(object));
                        Func<object, object> converter = Expression.Lambda<Func<object, object>>(
                            Expression.Convert(Expression.Convert(Expression.Convert(p, value.GetType()), property.PropertyType), typeof(object)),
                            p
                            ).Compile();
                        _converters.Add(convName, converter);
                    }
                    return _converters[convName](value);
                }
                else
                    return value;
            }
        }
    }
}
