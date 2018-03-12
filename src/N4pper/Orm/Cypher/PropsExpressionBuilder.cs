using Newtonsoft.Json;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.Orm.Cypher
{
    public abstract class PropsExpressionBuilder<T> : TypedStatementBuilder<T> where T : class
    {
        public override string Template { get; } = "{#props#}";

        protected override IDictionary<string, string> Variables { get; } = new Dictionary<string, string>()
        {
            { "#props#", ""}
        };

        protected Dictionary<string, Tuple<object, string, string>> Props { get; } = new Dictionary<string, Tuple<object, string, string>>();

        protected bool IsValue(string name)
        {
            return Props[name].Item1 != null;
        }
        protected bool IsParam(string name)
        {
            return Props[name].Item2 != null;
        }
        protected bool IsSymbol(string name)
        {
            return Props[name].Item3 != null;
        }

        protected void CheckName(string name)
        {
            if (typeof(T).GetProperty(name) == null)
                throw new ArgumentException($"The type {typeof(T).FullName} has not a property named {name}", nameof(name));
            if (ObjectExtensions.IsPrimitive(typeof(T).GetProperty(name).PropertyType) == false)
                throw new ArgumentException($"The property named {name}, is not of a primitive type", nameof(name));
        }

        public void SetValue(string name, object value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("cannot be null or empty", nameof(name));
            CheckName(name);

            if (!Props.ContainsKey(name))
                Props.Add(name, new Tuple<object, string, string>(value, null, null));
            else
                Props[name] = new Tuple<object, string, string>(value, null, null);
        }
        public object SetParam(string name, string paramName = null)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("cannot be null or empty", nameof(name));
            CheckName(name);

            paramName = paramName ?? "";
            if (!Props.ContainsKey(name))
            {
                Props.Add(name, new Tuple<object, string, string>(null, $"${paramName}", null));
                return null;
            }
            else
            {
                object prev = Props[name].Item1;
                Props[name] = new Tuple<object, string, string>(null, $"${paramName}", null);
                return prev;
            }
        }
        public void SetSymbol(string name, string symbol)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("cannot be null or empty", nameof(name));
            if (string.IsNullOrEmpty(symbol)) throw new ArgumentException("cannot be null or empty", nameof(symbol));
            CheckName(name);

            if (!Props.ContainsKey(name))
                Props.Add(name, new Tuple<object, string, string>(null, null, symbol));
            else
                Props[name] = new Tuple<object, string, string>(null, null, symbol);
        }

        public void SetValues(object values)
        {
            SetValues(values?.ToPropDictionary());
        }
        public void SetValues(Dictionary<string, object> values)
        {
            values = values ?? new Dictionary<string, object>();
            
            foreach (KeyValuePair<string, object> kv in values)
            {
                SetValue(kv.Key, kv.Value);
            }
        }

        public Dictionary<string, object> Parametrize(string suffix = null)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            suffix = suffix ?? "";
            foreach (string k in Props.Keys.ToList())
            {
                if (IsValue(k))
                {
                    result.Add($"{k}{suffix}", SetParam(k, $"{k}{suffix}"));
                }
            }
            return result;
        }

        public void TrimToKey()
        {
            if (OrmCoreTypes.KnownTypes.ContainsKey(typeof(T)))
            {
                foreach (string key in Props.Keys.ToList())
                {
                    if (OrmCoreTypes.KnownTypes[typeof(T)].Contains(key))
                        Props.Remove(key);
                }
            }
        }

        protected virtual KeyValuePair<string, string> ExtractKV(KeyValuePair<string, Tuple<object, string, string>> obj)
        {
            string value = obj.Value.Item3 ?? obj.Value.Item2 ?? (obj.Value.Item1 == null ? "null" : JsonConvert.SerializeObject(obj.Value.Item1));

            return new KeyValuePair<string, string>(obj.Key, value);
        }
        protected abstract string KVExpression(KeyValuePair<string, string> obj);

        public override string Build()
        {
            Variables["#props#"] = string.Join(
                ",",
                Props
                    .Select(p => KVExpression(ExtractKV(p)))
                    .ToArray());

            if (Variables["#props#"] == "")
                return "";
            else
                return base.Build();
        }
    }
}
