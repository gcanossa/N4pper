using Microsoft.Extensions.DependencyInjection;
using N4pper.Diagnostic;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Neo4j.Driver.V1;
using N4pper.Ogm.Core;

namespace N4pper.Ogm
{
    public static class ServiceCollectionExtensions
    {
        #region nested types
        
        public interface IGraphContextConfigurator
        {
            IServiceCollection AddGraphContext<T>(string Uri, IAuthToken authToken = null, Config config = null) where T : GraphContext;
        }
        public interface IGraphContextBuilder
        {
            void UseProvider();
        }
        private class GraphContextConfigurator : IGraphContextConfigurator
        {
            public IServiceCollection Services { get; set; }
            public GraphContextConfigurator(IServiceCollection services)
            {
                Services = services;
            }
            public IServiceCollection AddGraphContext<T>(string uri, IAuthToken authToken = null, Config config = null) where T : GraphContext
            {
                authToken = authToken ?? AuthTokens.None;
                config = config ?? new Config();
                Services.AddSingleton<DriverProvider<T>>(
                    provider => 
                    new InternalDriverProvider<T>(
                        provider.GetRequiredService<N4pperManager>()) { _Uri=uri, _AuthToken = authToken, _Config = config });
                Services.AddSingleton<T>();

                return Services;
            }
        }

        private class InternalDriverProvider<T> : DriverProvider<T> where T : GraphContext
        {
            public InternalDriverProvider(N4pperManager manager) : base(manager)
            {
            }

            public string _Uri { get; internal set; }
            public IAuthToken _AuthToken { get; internal set; }
            public Config _Config { get; internal set; }

            public override string Uri => _Uri;
            public override IAuthToken AuthToken => _AuthToken;
            public override Config Config => _Config;
        }

        #endregion

        public static IGraphContextConfigurator AddN4pperOgm(this IServiceCollection ext)
        {
            ext = ext ?? throw new ArgumentNullException(nameof(ext));

            ext.AddN4pper();
            ext.AddSingleton<EntityManagerBase>(
                new EntityManagerSelector(
                    new CypherEntityManager(), 
                    new Dictionary<Func<IStatementRunner, bool>, EntityManagerBase>()
                    {
                        { p => (p as IGraphManagedStatementRunner)?.IsApocAvailable ?? false, new ApocEntityManager() }
                    }));

            return new GraphContextConfigurator(ext);
        }
    }
}
