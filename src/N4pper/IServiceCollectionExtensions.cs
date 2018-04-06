using Microsoft.Extensions.DependencyInjection;
using N4pper.Diagnostic;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddN4pper(this IServiceCollection ext)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            ext.AddTransient<IQueryProfiler, DebugQueryProfiler>();
            ext.AddTransient<IQueryParamentersMangler, DefaultParameterMangler>();
            ext.AddTransient<IRecordHandler, DefaultRecordHanlder>();
            ext.AddTransient<OMnG.ObjectExtensionsConfiguration, N4pper.ObjectExtensionsConfiguration>();
            ext.AddTransient<OMnG.TypeExtensionsConfiguration, N4pper.TypeExtensionsConfiguration>();

            ManagerAccess.ServiceCollection = ext;

            return ext;
        }
    }
}
