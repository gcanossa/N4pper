using N4pper.Diagnostic;
using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace N4pper
{
    public class N4pperManager
    {
        public N4pperOptions Options { get; protected set; }

        public IQueryProfiler Profiler { get; protected set; }
        public IQueryParamentersMangler ParamentersMangler { get; protected set; }
        public IRecordHandler RecordHandler { get; protected set; }

        public OMnG.ObjectExtensionsConfiguration ObjectExtensionsConfiguration { get; protected set; }
        public OMnG.TypeExtensionsConfiguration TypeExtensionsConfiguration { get; protected set; }

        public N4pperManager(
            N4pperOptions options, 
            IQueryProfiler profiler, 
            IQueryParamentersMangler paramMangler, 
            IRecordHandler recordHandler,
            OMnG.ObjectExtensionsConfiguration objExtConf,
            OMnG.TypeExtensionsConfiguration typeExtConf)
        {
            Options = options ?? new N4pperOptions();
            Profiler = profiler;
            ParamentersMangler = paramMangler ?? new DefaultParameterMangler();
            RecordHandler = recordHandler ?? new DefaultRecordHanlder();
            ObjectExtensionsConfiguration = objExtConf ?? new N4pper.ObjectExtensionsConfiguration();
            TypeExtensionsConfiguration = typeExtConf ?? new N4pper.TypeExtensionsConfiguration();
        }
        
        public IStatementResult ProfileQuery(string statement, Func<IStatementResult> query)
        {
            Action<Exception> time = Profiler?.Mark(statement);
            try
            {
                IStatementResult r = query();
                time?.Invoke(null);
                return r;
            }
            catch(Exception e)
            {
                time?.Invoke(e);
                throw e;
            }
        }
        public Task<IStatementResultCursor> ProfileQueryAsync(string statement, Func<Task<IStatementResultCursor>> query)
        {
            Action<Exception> time = Profiler?.Mark(statement);
            try
            {
                Task<IStatementResultCursor> r = query();
                r.ContinueWith(p =>
                {
                    if(p.Exception == null)
                        time?.Invoke(null);
                    else
                        time?.Invoke(p.Exception);
                });
                return r;
            }
            catch (Exception e)
            {
                time?.Invoke(e);
                throw e;
            }
        }

        public IDisposable ScopeOMnG()
        {
            IDisposable obj = ObjectExtensions.ConfigScope(ObjectExtensionsConfiguration);
            IDisposable tpy = TypeExtensions.ConfigScope(TypeExtensionsConfiguration);

            return new CustomDisposable(()=> 
            {
                obj?.Dispose();
                tpy?.Dispose();
            });
        }
    }
}
