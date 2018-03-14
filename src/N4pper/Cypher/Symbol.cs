using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace N4pper.Cypher
{
    public class Symbol
    {
        public static implicit operator string(Symbol obj)
        {
            return obj?.ToString();
        }
        public static implicit operator Symbol(string obj)
        {
            return new Symbol(obj);
        }
        protected string Value { get; set; }
        internal Symbol(string value)
        {
            value = value ?? throw new ArgumentNullException(nameof(value));
            if (!Regex.IsMatch(value, "^[a-zA-Z_][\\w\\$]*"))
                throw new ArgumentException("invalid format", nameof(value));

            Value = value;
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
