using Microsoft.Extensions.DependencyInjection;
using N4pper.Diagnostic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddN4pper(this IServiceCollection ext)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            if (!ext.Any(p => p.ServiceType == typeof(N4pperManager)))
                ext.AddSingleton<N4pperManager, N4pperManager>();
            if (!ext.Any(p => p.ServiceType == typeof(N4pperOptions)))
                ext.AddSingleton<N4pperOptions, N4pperOptions>();
            if (!ext.Any(p => p.ServiceType == typeof(IQueryProfiler)))
                ext.AddSingleton<IQueryProfiler, DebugQueryProfiler>();
            if (!ext.Any(p => p.ServiceType == typeof(IQueryParamentersMangler)))
                ext.AddSingleton<IQueryParamentersMangler, DefaultParameterMangler>();
            if (!ext.Any(p => p.ServiceType == typeof(IRecordHandler)))
                ext.AddSingleton<IRecordHandler, DefaultRecordHanlder>();
            if (!ext.Any(p => p.ServiceType == typeof(OMnG.ObjectExtensionsConfiguration)))
                ext.AddSingleton<OMnG.ObjectExtensionsConfiguration, N4pper.ObjectExtensionsConfiguration>();
            if (!ext.Any(p => p.ServiceType == typeof(OMnG.TypeExtensionsConfiguration)))
                ext.AddSingleton<OMnG.TypeExtensionsConfiguration, N4pper.TypeExtensionsConfiguration>();

            ManagerAccess.ServiceCollection = ext;

            return ext;
        }
    }
}
