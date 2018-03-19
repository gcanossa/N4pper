using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace N4pper.QueryUtils
{
    public class Parameter
    {
        public static implicit operator string(Parameter obj)
        {
            return obj?.ToString();
        }
        public static implicit operator Parameter(string obj)
        {
            return new Parameter(obj);
        }
        protected string Value { get; set; }
        internal Parameter(string value)
        {
            value = value ?? throw new ArgumentNullException(nameof(value));
            if (!Regex.IsMatch(value, "^[\\w\\$]*"))
                throw new ArgumentException("invalid format", nameof(value));

            Value = value.StartsWith("$") ? value : $"${value}";
        }
        public override string ToString()
        {
            return Value;
        }
        public override bool Equals(object obj)
        {
            return Value.Equals(obj?.ToString());
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
