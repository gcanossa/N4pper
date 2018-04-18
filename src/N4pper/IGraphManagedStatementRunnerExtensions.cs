using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public static class IGraphManagedStatementRunnerExtensions
    {
        public static IGraphManagedStatementRunner AsManaged(this IStatementRunner ext)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            return (ext as IGraphManagedStatementRunner) ?? throw new ArgumentException("Unable to manage runner", nameof(ext));
        }

        public static IDictionary<string, object> MangleParameters(this IGraphManagedStatementRunner ext, object parameters)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            return ext.Manager.ParamentersMangler.Mangle(
                parameters is IDictionary<string, object> ? 
                (IDictionary<string, object>)parameters : parameters?.ToPropDictionary());
        }
        public static object ParseRecord(this IGraphManagedStatementRunner ext, object value, Type assigningType, Type realType)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));
            assigningType = assigningType ?? throw new ArgumentNullException(nameof(assigningType));
            realType = realType ?? throw new ArgumentNullException(nameof(realType));

            if (value == null)
                return null;
            else
                return ext.Manager.RecordHandler.ParseRecordValue(value, assigningType, realType);
        }
    }
}
