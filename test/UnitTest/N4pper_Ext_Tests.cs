using AsIKnow.XUnitExtensions;
using N4pper;
using N4pper.Ogm;
using N4pper.QueryUtils;
using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitTest.TestModel;
using Xunit;
using static UnitTest.Neo4jFixture;

namespace UnitTest
{
    [TestCaseOrderer(AsIKnow.XUnitExtensions.Constants.PriorityOrdererTypeName, AsIKnow.XUnitExtensions.Constants.PriorityOrdererTypeAssemblyName)]
    [Collection(nameof(Neo4jCollection))]
    public class N4pper_Ext_Tests
    {
        protected Neo4jFixture Fixture { get; set; }

        public N4pper_Ext_Tests(Neo4jFixture fixture)
        {
            Fixture = fixture;
        }

        private Dictionary<string, object> DefaultTestEntity = new Dictionary<string, object>()
        {
            { nameof(ITestEntity.Integer), 1},
            { nameof(ITestEntity.IntegerNullable), (int?)1},
            { nameof(ITestEntity.Double), 1.1},
            { nameof(ITestEntity.DateTime), new DateTime(1970, 1, 31)},
            { nameof(ITestEntity.DateTimeNullable), new DateTime(1970, 1, 31)},
            { nameof(ITestEntity.DateTimeOffset), (DateTimeOffset)new DateTime(1970, 1, 31)},
            { nameof(ITestEntity.DateTimeOffsetNullable), (DateTimeOffset)new DateTime(1970, 1, 31)},
            { nameof(ITestEntity.TimeSpan), new TimeSpan(1, 1, 1)},
            { nameof(ITestEntity.TimeSpanNullable), new TimeSpan(1, 1, 1)},
            { nameof(ITestEntity.String), "String"},
            { nameof(ITestEntity.Object), new object()}
        };

        private void PrepareEntity(ITestEntity entity)
        {
            entity.CopyProperties(DefaultTestEntity);

            entity.WriteonlyInt = 1;
        }
        private void CheckEntityEquals(ITestEntity entity)
        {
            Assert.Equal(DefaultTestEntity[nameof(ITestEntity.Integer)], entity.Integer);
            Assert.Equal(DefaultTestEntity[nameof(ITestEntity.IntegerNullable)], entity.IntegerNullable);
            Assert.Equal(DefaultTestEntity[nameof(ITestEntity.Double)], entity.Double);
            Assert.Equal(DefaultTestEntity[nameof(ITestEntity.DateTime)], entity.DateTime);
            Assert.Equal(DefaultTestEntity[nameof(ITestEntity.DateTimeNullable)], entity.DateTimeNullable);
            Assert.Equal(DefaultTestEntity[nameof(ITestEntity.DateTimeOffset)], entity.DateTimeOffset);
            Assert.Equal(DefaultTestEntity[nameof(ITestEntity.DateTimeOffsetNullable)], entity.DateTimeOffsetNullable);
            Assert.Equal(DefaultTestEntity[nameof(ITestEntity.TimeSpan)], entity.TimeSpan);
            Assert.Equal(DefaultTestEntity[nameof(ITestEntity.TimeSpanNullable)], entity.TimeSpanNullable);
            Assert.Equal(DefaultTestEntity[nameof(ITestEntity.String)], entity.String);
            Assert.Equal(DefaultTestEntity[nameof(ITestEntity.Object)], entity.Object);
        }
        private void CheckEntityEquals(ITestEntity entity, ITestEntity entity2)
        {
            Assert.Equal(entity.Integer, entity2.Integer);
            Assert.Equal(entity.IntegerNullable, entity2.IntegerNullable);
            Assert.Equal(entity.Double, entity2.Double);
            Assert.Equal(entity.DateTime, entity2.DateTime);
            Assert.Equal(entity.DateTimeNullable, entity2.DateTimeNullable);
            Assert.Equal(entity.DateTimeOffset, entity2.DateTimeOffset);
            Assert.Equal(entity.DateTimeOffsetNullable, entity2.DateTimeOffsetNullable);
            Assert.Equal(entity.TimeSpan, entity2.TimeSpan);
            Assert.Equal(entity.TimeSpanNullable, entity2.TimeSpanNullable);
            Assert.Equal(entity.String, entity2.String);
        }


        [TestPriority(0)]
        [Trait("Category", nameof(N4pper_Ext_Tests))]
        [Fact(DisplayName = nameof(GetDriver))]
        public void GetDriver()
        {
            DriverProvider provider = Fixture.GetService<DriverProvider<GlobalTestContext>>();

            var x = provider.GetDriver();
            Assert.NotNull(x);
            Assert.Equal(x, provider.GetDriver());
        }

        [TestPriority(10)]
        [Trait("Category", nameof(N4pper_Ext_Tests))]
        [Fact(DisplayName = nameof(CRUD_Node))]
        public void CRUD_Node()
        {
            DriverProvider provider = Fixture.GetService<DriverProvider<GlobalTestContext>>();

            using (ISession session = provider.GetDriver().Session())
            {
                Symbol s = new Symbol();
                TestNode tnode = new TestNode() { Id = 1 };
                PrepareEntity(tnode);

                TestNode val = session.ExecuteQuery<TestNode>($"MATCH {new Node(s, tnode.GetType(), new { Id = 1 }.ToPropDictionary())} RETURN {s}").FirstOrDefault();
                Assert.Null(val);

                val = session.ExecuteQuery<TestNode>($"CREATE " +
                    $"{new Node(s, tnode.GetType(), (IDictionary<string, object>)tnode.Scope(new N4pper.ObjectExtensionsConfiguration(), q=>q.ToPropDictionary()))}" +
                    $" RETURN {s}").FirstOrDefault();
                Assert.NotNull(val);

                Assert.Equal(tnode.Id, val.Id);
                CheckEntityEquals(tnode, val);

                val = session.ExecuteQuery<TestNode>($"MATCH {new Node(s, tnode.GetType(), new { Id = 1 }.ToPropDictionary())} SET {new Set(s, new { Double = 1.2 }.ToPropDictionary())} RETURN {s}")
                    .FirstOrDefault();
                Assert.NotNull(val);
                Assert.Equal(1.2, val.Double);

                IResultSummary result = session.Execute($"MATCH {new Node(s, tnode.GetType(), new { Id = 1 }.ToPropDictionary())} DELETE {s}");
                Assert.Equal(1, result.Counters.NodesDeleted);

                val = session.ExecuteQuery<TestNode>($"MATCH {new Node(s, tnode.GetType(), new { Id = 1 }.ToPropDictionary())} RETURN {s}").FirstOrDefault();
                Assert.Null(val);
            }
        }
        [TestPriority(10)]
        [Trait("Category", nameof(N4pper_Ext_Tests))]
        [Fact(DisplayName = nameof(CRUD_Rel))]
        public void CRUD_Rel()
        {
            DriverProvider provider = Fixture.GetService<DriverProvider<GlobalTestContext>>();

            using (ISession session = provider.GetDriver().Session())
            {
                Symbol s = new Symbol();
                TestRel trel = new TestRel() { Id = 1 };
                PrepareEntity(trel);

                TestRel val = session.ExecuteQuery<TestRel>(p => $"MATCH {new Node()._(s, typeof(TestRel), new { Id = 1 }.ToPropDictionary())._V()} RETURN {s}").FirstOrDefault();
                Assert.Null(val);

                val = session.ExecuteQuery<TestRel>($"CREATE (p) WITH p CREATE (q) WITH p,q CREATE (p)-" +
                    $"{new Rel(s, trel.GetType(), (IDictionary<string, object>)trel.Scope(new N4pper.ObjectExtensionsConfiguration(), q => q.ToPropDictionary()))}" +
                    $"->(q) RETURN {s}").FirstOrDefault();
                Assert.NotNull(val);

                Assert.Equal(trel.Id, val.Id);
                CheckEntityEquals(trel, val);

                val = session.ExecuteQuery<TestRel>($"MATCH {new Node()._(s, typeof(TestRel), new { Id = 1 }.ToPropDictionary())._V()} SET {new Set(s, new { Double = 1.2 }.ToPropDictionary())} RETURN {s}")
                    .FirstOrDefault();
                Assert.NotNull(val);
                Assert.Equal(1.2, val.Double);

                Symbol p1 = new Symbol();
                Symbol p2 = new Symbol();
                IResultSummary result = session.Execute($"MATCH {new Node(p1)._(s, typeof(TestRel), new { Id = 1 }.ToPropDictionary())._V(p2)} DELETE {s},{p1},{p2}");
                Assert.Equal(2, result.Counters.NodesDeleted);
                Assert.Equal(1, result.Counters.RelationshipsDeleted);

                val = session.ExecuteQuery<TestRel>(p => $"MATCH {new Node()._(s, typeof(TestRel), new { Id = 1 }.ToPropDictionary())._V()} RETURN {s}").FirstOrDefault();
                Assert.Null(val);
            }
        }

        [TestPriority(20)]
        [Trait("Category", nameof(N4pper_Ext_Tests))]
        [Fact(DisplayName = nameof(CRUD_Multi))]
        public void CRUD_Multi()
        {
            DriverProvider provider = Fixture.GetService<DriverProvider<GlobalTestContext>>();

            using (ISession session = provider.GetDriver().Session())
            {
                session.Execute(p=>$"CREATE {p.Node<Book>(p.Symbol(), new { EntityId = 1})}");
                session.Execute(p => $"CREATE {p.Node<Chapter>(p.Symbol(), new { EntityId = 1 })}");
                session.Execute(p => $"CREATE {p.Node<Chapter>(p.Symbol(), new { EntityId = 2 })}");

                Symbol b = new Symbol();
                Symbol c = new Symbol();
                session.Execute(p => $"MATCH {p.Node<Book>(b, new { EntityId = 1 }).BuildForQuery()}, {p.Node<Chapter>(c).BuildForQuery()} " +
                $"CREATE {new Node(b)}-[:rel]->{new Node(c)}");

                Book test = session.ExecuteQuery<Book, IEnumerable<Chapter>>(p=> $"MATCH {p.Node<Book>(b)}-[:rel]->{p.Node<Chapter>(c)} RETURN {b}, collect({c}) AS c",
                    (book, chapters) =>
                    {
                        book.Chapters = book.Chapters??new List<Chapter>();
                        foreach (Chapter item in chapters.OrderBy(p=>p.EntityId))
                        {
                            book.Chapters.Add(item);
                        }
                        return book;
                    }).FirstOrDefault();

                Assert.NotNull(test);

                Assert.Equal(2, test.Chapters.Count);
                Assert.Equal(1, test.Chapters[0].EntityId);
                Assert.Equal(2, test.Chapters[1].EntityId);

                IResultSummary result = session.Execute(p => $"MATCH {p.Node<Book>(b)}-[:rel]->{p.Node<Chapter>(c)} DETACH DELETE {b}, {c}");
                Assert.Equal(3, result.Counters.NodesDeleted);
                Assert.Equal(2, result.Counters.RelationshipsDeleted);
            }
        }
    }
}
