using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.QueryUtils
{
    public abstract class EntityBase : IEntity, IStatementBuilder
    {
        public static implicit operator string(EntityBase entity)
        {
            return entity?.Build();
        }
        public virtual Symbol Symbol { get; protected set; }
        public virtual Type Type { get; protected set; }
        public virtual IDictionary<string, object> Props { get; protected set; }

        public EntityBase(Symbol symbol = null, Type type = null, IDictionary<string, object> props = null)
        {
            Symbol = symbol;
            Type = type;
            Props = props ?? new Dictionary<string, object>();
        }

        public abstract string Build();

        protected virtual string HandleValue(object value)
        {
            if (value is Symbol)
            {
                return value.ToString();
            }
            else if (value is Parameter)
            {
                return value.ToString();
            }

            if (value == null)
                return "null";
            else
            {
                if (ObjectExtensions.IsDateTime(value.GetType()))
                {
                    DateTimeOffset d = (DateTime)value;
                    return d.ToUnixTimeMilliseconds().ToString();
                }
                else if (value.GetType() == typeof(TimeSpan) || value.GetType() == typeof(TimeSpan?))
                {
                    return ((TimeSpan)value).TotalMilliseconds.ToString();
                }
                else if (Type.GetTypeCode(value.GetType()) == TypeCode.String)
                    return $"'{value}'";
                else if (ObjectExtensions.IsPrimitive(value.GetType()))
                    return value.ToString();
                else
                    throw new ArgumentException($"Unsupported type: '{value.GetType().FullName}'", nameof(value));
            }
        }

        protected virtual string SerializeProps()
        {
            return "{" + string.Join(",", Props.Select(p => $"{p.Key}:{HandleValue(p.Value)}")) + "}";
        }

        public override string ToString()
        {
            return Build();
        }

        public Parameters Parametrize(string suffix = null)
        {
            Parameters p = new Parameters(Props.Keys, suffix);
            p.Apply(this);

            return p;
        }

        public IEntity Parametrize(Parameters p)
        {
            p = p ?? throw new ArgumentNullException(nameof(p));

            p.Apply(this);

            return this;
        }
    }
}
