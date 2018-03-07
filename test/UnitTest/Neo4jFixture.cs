using AsIKnow.XUnitExtensions;
using Microsoft.Extensions.DependencyInjection;
using AsIKnow.DependencyHelpers.Neo4j;
using System;
using System.Collections.Generic;
using System.Text;
using Neo4j.Driver.V1;
using AsIKnow.Graph;
using N4pper;
using N4pper.Diagnostic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UnitTest
{
    public interface Neo4jServer
    { }

    public class Neo4jFixture : DockerEnvironmentsBaseFixture<Neo4jServer>
    {
        protected override void ConfigureServices(ServiceCollection sc)
        {
            sc.AddSingleton<TypeManagerOptions>(new TypeManagerOptions());

            sc.AddSingleton<N4pperOptions>(new N4pperOptions());
            sc.AddSingleton<TypeManager, ReflectionTypeManager>();
            sc.AddSingleton<GraphManager>();

            sc.AddTransient<IQueryTracer, QueryTraceLogger>();

            sc.AddTransient<IDriver>(s => GraphDatabase.Driver(new Uri(Configuration.GetConnectionString("DefaultConnection")), AuthTokens.None));

            sc.AddLogging(builder => builder.AddDebug());
        }

        public void Configure()
        {
            WaitForDependencies(builder => builder.AddNeo4jServer(new Uri(Configuration.GetConnectionString("DefaultConnection")), AuthTokens.None, "test"));
        }
    }
}
