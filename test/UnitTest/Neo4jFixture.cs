using AsIKnow.XUnitExtensions;
using Microsoft.Extensions.DependencyInjection;
using AsIKnow.DependencyHelpers.Neo4j;
using System;
using System.Collections.Generic;
using System.Text;
using Neo4j.Driver.V1;
using N4pper;
using N4pper.Diagnostic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using N4pper.Orm;
using N4pper.Orm.Design;

namespace UnitTest
{
    public interface Neo4jServer
    { }

    public class Neo4jFixture : DockerEnvironmentsBaseFixture<Neo4jServer>
    {
        protected override void ConfigureServices(ServiceCollection sc)
        {
            sc.AddSingleton<IConfigurationRoot>(Configuration);
            sc.AddN4pper()
                .AddGraphContext<TestContext>(Configuration.GetConnectionString("DefaultConnection"));
            sc.AddN4pper()
                .AddGraphContext<GlobalTestContext>(Configuration.GetConnectionString("DefaultConnection"));

            sc.AddTransient<IQueryProfiler, QueryTraceLogger>();

            sc.AddTransient<Neo4jServer_DriverBuilder>(provider => new Neo4jServer_DriverBuilder(Configuration));
            //sc.AddTransient<TestContext>();
            //sc.AddTransient<DriverProvider<TestContext>, Neo4jServer_DriverProvider>();
            //sc.AddTransient<IDriver>(s => GraphDatabase.Driver(new Uri(Configuration.GetConnectionString("DefaultConnection")), AuthTokens.None));

            sc.AddLogging(builder => builder.AddDebug().AddConsole());
        }

        public class GlobalTestContext : GraphContext
        {
            public GlobalTestContext(DriverProvider<TestContext> provider) : base(provider)
            {
            }
            protected override void OnModelCreating(GraphModelBuilder builder)
            {
                base.OnModelCreating(builder);

                builder.Entity<TestModel.Book>()
                    .ConnectedMany(p => p.Chapters).Connected(p=>p.Book);
                builder.Entity<TestModel.Chapter>();
                builder.Entity<TestModel.Section>();
                builder.ConnectionEntity<TestModel.Friend>();
                builder.Entity<TestModel.User>()
                    .ConnectedManyWith<TestModel.Friend, TestModel.User>(p => p.Friends).ConnectedMany(p => p.Friends);
                builder.Entity<TestModel.User>()
                    .ConnectedWith<TestModel.Friend, TestModel.User>(p=>p.BestFriend).Connected(p=>p.BestFriend);
                builder.Entity<TestModel.User>()
                    .Ignore(p=>p.Age);
                builder.Entity<TestModel.Exercise>();
                builder.Entity<TestModel.Explaination>();
            }
        }

        public class TestContext : GraphContext
        {
            public TestContext(DriverProvider<TestContext> provider) : base(provider)
            {
            }
            protected override void OnModelCreating(GraphModelBuilder builder)
            {
                base.OnModelCreating(builder);

                builder.Entity<OrmCoreTests.PersonX>();
                builder.Entity<OrmCoreTests.Student>(p => p.Id);
                builder.Entity<OrmCoreTests.Teacher>();
                builder.Entity<OrmCoreTests.Class>();

                builder.Entity<OrmCoreTests.Question>();
                builder.Entity<OrmCoreTests.Suggestion>();
                builder.Entity<OrmCoreTests.ContentPersonRel>();


                builder.Entity<UnitTest.Types.Person>();
                builder.Entity<UnitTest.Types.Child>();
                builder.Entity<UnitTest.Types.Parent>();
            }
        }

        //public class Neo4jServer_DriverProvider : DriverProvider<TestContext>
        //{
        //    private IConfigurationRoot _conf;
        //    public Neo4jServer_DriverProvider(IConfigurationRoot conf, N4pperManager manager)
        //        :base(manager)
        //    {
        //        _conf = conf;
        //    }

        //    public override string Uri => _conf.GetConnectionString("DefaultConnection");

        //    public override IAuthToken AuthToken => AuthTokens.None;

        //    public override Config Config => new Config();
        //}
        public class Neo4jServer_DriverBuilder : DriverBuilder
        {
            private IConfigurationRoot _conf;
            public Neo4jServer_DriverBuilder(IConfigurationRoot conf)
            {
                _conf = conf;
            }
            public override string Uri => _conf.GetConnectionString("DefaultConnection");

            public override IAuthToken AuthToken => AuthTokens.None;

            public override Config Config => new Config();
        }

        public void Configure()
        {
            WaitForDependencies(builder => builder.AddNeo4jServer<Neo4jServer_DriverBuilder>("test"));
        }
    }
}
