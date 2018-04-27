using OMnG;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        public virtual string BuildForQuery()
        {
            return Build();
        }

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
                if (value.GetType().IsDateTime())
                {
                    DateTimeOffset d = value is DateTimeOffset ? (DateTimeOffset)value : (DateTime)value;
                    return d.ToUnixTimeMilliseconds().ToString();
                }
                else if (value.GetType().IsTimeSpan())
                {
                    return ((TimeSpan)value).TotalMilliseconds.ToString();
                }
                else if (Type.GetTypeCode(value.GetType()) == TypeCode.String)
                    return $"'{value}'";
                else if (value.GetType().IsEnum)
                    return $"{(int)value}";
                else if (value.GetType().IsPrimitive())
                {
                    NumberFormatInfo nfi = new NumberFormatInfo();
                    nfi.NumberDecimalSeparator = ".";
                    return string.Format(nfi, "{0}", value);
                }
                else
                    throw new ArgumentException($"Unsupported type: '{value.GetType().FullName}'", nameof(value));
            }
        }

        protected virtual string SerializeProps()
        {
            using (ManagerAccess.Manager.ScopeOMnG())
            {
                return "{" + string.Join(",", Props
                    .Where(
                    p =>
                        Type == null ||
                        p.Value == null ||
                        p.Value is Parameter ||
                        p.Value is Symbol ||
                        ((p.Value.GetType().IsPrimitive() || p.Value.GetType().IsEnum) && (Type.GetProperty(p.Key)?.CanRead ?? true) && (Type.GetProperty(p.Key)?.CanWrite ?? true))
                        )
                    .Select(p => $"{p.Key}:{HandleValue(p.Value)}")) + "}";
            }
        }

        public override string ToString()
        {
            return Build();
        }

        public Parameters Parametrize(string suffix = null, string prefix = null)
        {
            Parameters p = new Parameters(Props.Keys, suffix, prefix);
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
