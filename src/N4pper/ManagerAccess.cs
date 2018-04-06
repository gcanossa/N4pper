using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    internal class ManagerAccess
    {
        private static IServiceCollection _serviceCollection;
        internal static IServiceCollection ServiceCollection
        {
            get
            {
                return _serviceCollection;
            }
            set
            {
                if (_manager != null)
                    throw new InvalidOperationException("The manager is already been created");
                _serviceCollection = value;
            }
        }
        public static IServiceProvider Provider
        {
            get
            {
                return ServiceCollection?.BuildServiceProvider();
            }
        }

        private static N4pperManager _manager;
        public static N4pperManager Manager
        {
            get
            {
                if (Provider != null)
                    return Provider.GetRequiredService<N4pperManager>();
                else if (_manager == null)
                    _manager = new N4pperManager(new N4pperOptions(), null, null, null, null, null);
                return _manager;
            }
        }
    }
}
