using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace N4pper.Ogm.Design
{
    internal class KnownTypeDescriptor
    {
        public List<PropertyInfo> IgnoredProperties { get; } = new List<PropertyInfo>();
    }
}
