using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public class TypeExtensionsConfiguration : OMnG.TypeExtensionsConfiguration.DefaultConfiguration
    {
        public override bool FilterValidType(Type type)
        {
            return base.FilterValidType(type) && type.Assembly != typeof(Castle.DynamicProxy.ProxyGenerator).Assembly;
        }
    }
}
