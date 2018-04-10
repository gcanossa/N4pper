using AsIKnow.XUnitExtensions;
using N4pper;
using N4pper.Ogm;
using N4pper.Ogm.Core;
using N4pper.Ogm.Design;
using N4pper.Ogm.Entities;
using N4pper.QueryUtils;
using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitTest.TestModel;
using Xunit;
using Moq;

namespace UnitTest
{
    public class N4pper_Ogm_MockTests
    {
        #region nested types

        public class MockDriverProvider : DriverProvider
        {
            protected IDriver _driver;
            public MockDriverProvider(IDriver driver) : base(new N4pperManager(new N4pperOptions(), new QueryTraceLogger(), new DefaultParameterMangler(), new DefaultRecordHanlder(), new N4pper.ObjectExtensionsConfiguration(), new N4pper.TypeExtensionsConfiguration()))
            {
                _driver = driver;
            }

            public override IDriver GetDriver()
            {
                return _driver;
            }

            public override string Uri => null;

            public override IAuthToken AuthToken => null;

            public override Config Config => null;
        }

        public class MockContext : GraphContext
        {
            public MockContext(DriverProvider provider, TypesManager typesManager, ChangeTrackerBase changeTracker, EntityManagerBase entityManager)
                : base(provider, typesManager, changeTracker, entityManager)
            {
            }
            protected override void OnModelCreating(GraphModelBuilder builder)
            {
                base.OnModelCreating(builder);

                builder.Entity<TestModel.Book>()
                    .ConnectedMany(p => p.Chapters).Connected(p => p.Book);
                builder.Entity<TestModel.Chapter>();
                builder.Entity<TestModel.Section>();
                builder.ConnectionEntity<TestModel.Friend>(true);
                builder.Entity<TestModel.User>()
                    .ConnectedManyWith<TestModel.Friend, TestModel.User>(p => p.Friends).ConnectedMany(p => p.Friends);
                builder.Entity<TestModel.User>()
                    .ConnectedWith<TestModel.Friend, TestModel.User>(p => p.BestFriend).Connected(p => p.BestFriend);
                builder.Entity<TestModel.User>()
                    .Ignore(p => p.Age);
                builder.Entity<TestModel.Exercise>();
                builder.Entity<TestModel.Explaination>();
            }
        }

        public class TestEntityManager : EntityManagerBase
        {
            protected long CurrentId { get; set; } = 0;

            public List<Tuple<IOgmEntity, IEnumerable<string>>> CreatedNodes = new List<Tuple<IOgmEntity, IEnumerable<string>>>();
            public List<Tuple<long, Tuple<IOgmEntity, IEnumerable<string>>, long>> CreatedRels = new List<Tuple<long, Tuple<IOgmEntity, IEnumerable<string>>, long>>();

            public List<IOgmEntity> DeletedNodes = new List<IOgmEntity>();
            public List<IOgmEntity> DeletedRels = new List<IOgmEntity>();

            public List<Tuple<IOgmEntity, IEnumerable<string>>> UpdatedNodes = new List<Tuple<IOgmEntity, IEnumerable<string>>>();
            public List<Tuple<IOgmEntity, IEnumerable<string>>> UpdatedRels = new List<Tuple<IOgmEntity, IEnumerable<string>>>();

            public List<Tuple<long, Tuple<Connection, IEnumerable<string>>, long>> ConnectionMerge = new List<Tuple<long, Tuple<Connection, IEnumerable<string>>, long>>();

            public override IEnumerable<IOgmEntity> CreateNodes(IStatementRunner runner, IEnumerable<Tuple<IOgmEntity, IEnumerable<string>>> entities)
            {
                CreatedNodes.Clear();
                CreatedNodes.AddRange(entities);

                foreach (var item in entities)
                {
                    item.Item1.EntityId = CurrentId++;
                }
                return entities.Select(p => p.Item1);
            }

            public override IEnumerable<IOgmEntity> CreateRels(IStatementRunner runner, IEnumerable<Tuple<long, Tuple<IOgmEntity, IEnumerable<string>>, long>> entities)
            {
                CreatedRels.Clear();
                CreatedRels.AddRange(entities);

                foreach (var item in entities)
                {
                    item.Item2.Item1.EntityId = CurrentId++;
                }
                return entities.Select(p => p.Item2.Item1);
            }

            public override void DeleteNodes(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
            {
                DeletedNodes.Clear();
                DeletedNodes.AddRange(entities);
            }

            public override void DeleteRels(IStatementRunner runner, IEnumerable<IOgmEntity> entities)
            {
                DeletedRels.Clear();
                DeletedRels.AddRange(entities);
            }

            public override IEnumerable<IOgmEntity> UpdateNodes(IStatementRunner runner, IEnumerable<Tuple<IOgmEntity, IEnumerable<string>>> entities)
            {
                UpdatedNodes.Clear();
                UpdatedNodes.AddRange(entities);

                return entities.Select(p => p.Item1);
            }

            public override IEnumerable<IOgmEntity> UpdateRels(IStatementRunner runner, IEnumerable<Tuple<IOgmEntity, IEnumerable<string>>> entities)
            {
                UpdatedRels.Clear();
                UpdatedRels.AddRange(entities);

                return entities.Select(p => p.Item1);
            }

            public override IEnumerable<Connection> MergeConnections(IStatementRunner runner, IEnumerable<Tuple<long, Tuple<Connection, IEnumerable<string>>, long>> entities)
            {
                ConnectionMerge.Clear();
                ConnectionMerge.AddRange(entities);

                return entities.Select(p => p.Item2.Item1);
            }
        }

        #endregion

        public GraphContext GetContext(EntityManagerBase entityManager = null)
        {
            var driver = new Mock<IDriver>();
            var driverProvider = new MockDriverProvider(driver.Object);
            var session = new Mock<ISession>();

            driver.Setup(p => p.Session()).Returns(() => session.Object);
            session.Setup(p => p.Dispose());

            return new MockContext(driverProvider, new TypesManager(), new DefaultChangeTracker(), entityManager ?? new TestEntityManager());
        }

        [TestPriority(10)]
        [Trait("Category", nameof(N4pper_Ogm_MockTests))]
        [Fact(DisplayName = nameof(CrUD_Nodes))]
        public void CrUD_Nodes()
        {
            TestEntityManager testManager = new TestEntityManager();
            using (GraphContext ctx = GetContext(testManager))
            {
                Symbol s = new Symbol();
                Book orgBook;
                Book book = new Book() { Name = "Prova" };
                orgBook = book;

                book = ctx.Add(book);

                book.Index = 1;

                Assert.Empty(testManager.CreatedNodes);

                ctx.SaveChanges();

                Assert.Equal(1, testManager.CreatedNodes.Count);
                Assert.Equal("Prova", testManager.CreatedNodes[0].Item1.GetPropValue("Name"));

                book.Name = "Prova 2";

                ctx.SaveChanges();

                Assert.Empty(testManager.CreatedNodes);
                Assert.Equal(1, testManager.UpdatedNodes.Count);
                Assert.Equal("Prova 2", testManager.UpdatedNodes[0].Item1.GetPropValue("Name"));
                Assert.True(testManager.UpdatedNodes[0].Item2.Count()>0 && !testManager.UpdatedNodes[0].Item2.Contains("Name"));
                
                ctx.Remove(book);
                
                ctx.SaveChanges();

                Assert.Empty(testManager.CreatedNodes);
                Assert.Empty(testManager.UpdatedNodes);
                Assert.Equal(1, testManager.DeletedNodes.Count);

                Assert.Equal(orgBook, testManager.DeletedNodes[0]);
            }
        }
    }
}
