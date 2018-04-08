using AsIKnow.XUnitExtensions;
using N4pper;
using N4pper.Ogm;
using N4pper.Ogm.Core;
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
using static UnitTest.Neo4jFixture;

namespace UnitTest.Types
{
    [TestCaseOrderer(AsIKnow.XUnitExtensions.Constants.PriorityOrdererTypeName, AsIKnow.XUnitExtensions.Constants.PriorityOrdererTypeAssemblyName)]
    [Collection(nameof(Neo4jCollection))]
    public class N4pper_Ogm_Core_Tests
    {
        protected Neo4jFixture Fixture { get; set; }

        public N4pper_Ogm_Core_Tests(Neo4jFixture fixture)
        {
            Fixture = fixture;
        }

        #region nested generators

        public static IEnumerable<object[]> GetEntityManagers()
        {
            yield return new[] { new CypherEntityManager() };
            yield return new[] { new ApocEntityManager() };
            yield return new[] { new EntityManagerSelector(new CypherEntityManager(), new Dictionary<Func<IStatementRunner, bool>, EntityManagerBase>() { { p=>(p as IGraphManagedStatementRunner)?.IsApocAvailable??false, new ApocEntityManager() } }) };
        }

        #endregion

        [TestPriority(10)]
        [Trait("Category", nameof(N4pper_Ogm_Core_Tests))]
        [Theory(DisplayName = nameof(CrUD_Nodes))]
        [MemberData(nameof(GetEntityManagers))]
        public void CrUD_Nodes(EntityManagerBase mgr)
        {
            DriverProvider provider = Fixture.GetService<DriverProvider<TestContext>>();

            using (ISession session = provider.GetDriver().Session())
            {
                Symbol s = new Symbol();
                
                string name = Guid.NewGuid().ToString("N");

                Book book = new Book() { Name = name };
                Chapter chapter = new Chapter() { Name = name };

                Assert.Equal(
                    0, 
                    session.ExecuteQuery<IOgmEntity>($"MATCH {new Node(s,type: typeof(IOgmEntity), props: new { Name = name }.ToPropDictionary()).BuildForQuery()} RETURN {s}")
                        .Count()
                    );

                List<IOgmEntity> res = mgr.CreateNodes(session, new IOgmEntity[] { book, chapter }).ToList();

                Assert.Equal(
                    2,
                    session.ExecuteQuery<IOgmEntity>($"MATCH {new Node(s, type: typeof(IOgmEntity), props: new { Name = name }.ToPropDictionary()).BuildForQuery()} RETURN {s}")
                        .Count()
                    );

                (res.First(p => p is Book) as Book).Index = 3;
                (res.First(p => p is Book) as Book).Name = null;
                (res.First(p => p is Chapter) as Chapter).Index = 4;

                List<IOgmEntity> res2 = mgr.UpdateNodes(session, res).ToList();

                Assert.Equal(
                    1,
                    session.ExecuteQuery<IOgmEntity>($"MATCH {new Node(s, type: typeof(IOgmEntity), props: new { Name = name }.ToPropDictionary()).BuildForQuery()} WHERE {s}.Index>0 RETURN {s}")
                        .Count()
                    );
                Assert.Equal(
                    1,
                    session.ExecuteQuery<IOgmEntity>($"MATCH {new Node(s, type: typeof(IOgmEntity), props: new { Index = 4 }.ToPropDictionary()).BuildForQuery()} WHERE {s}.Index>0 RETURN {s}")
                        .Count()
                    );

                mgr.DeleteNodes(session, res2);

                Assert.Equal(
                    0,
                    session.ExecuteQuery<IOgmEntity>($"MATCH {new Node(s, type: typeof(IOgmEntity), props: new { Name = name }.ToPropDictionary()).BuildForQuery()} RETURN {s}")
                        .Count()
                    );
            }
        }

        [TestPriority(10)]
        [Trait("Category", nameof(N4pper_Ogm_Core_Tests))]
        [Theory(DisplayName = nameof(CrUD_Rels))]
        [MemberData(nameof(GetEntityManagers))]
        public void CrUD_Rels(EntityManagerBase mgr)
        {
            DriverProvider provider = Fixture.GetService<DriverProvider<TestContext>>();

            using (ISession session = provider.GetDriver().Session())
            {
                Symbol s = new Symbol();
                
                string name = Guid.NewGuid().ToString("N");

                Book book = new Book() { Name = name };
                Chapter chapter = new Chapter() { Name = name };

                Assert.Equal(
                    0,
                    session.ExecuteQuery<IOgmEntity>($"MATCH {new Node(s, type: typeof(IOgmEntity), props: new { Name = name }.ToPropDictionary()).BuildForQuery()} RETURN {s}")
                        .Count()
                    );

                List<IOgmEntity> res = mgr.CreateNodes(session, new IOgmEntity[] { book, chapter }).ToList();

                List<IOgmEntity> rels = mgr.CreateRels(session, new Tuple<long, IOgmEntity, long> []{ new Tuple<long, IOgmEntity, long>(res[0].EntityId.Value, new Connection(), res[1].EntityId.Value) }).ToList();

                Assert.Equal(
                    1,
                    session.ExecuteQuery<IOgmEntity>($"MATCH ()-[{s} {{EntityId:{rels[0].EntityId}}}]->() RETURN {s}")
                        .Count()
                    );

                (rels[0] as Connection).SourcePropertyName = "test";

                List<IOgmEntity> rels2 = mgr.UpdateRels(session, rels).ToList();

                Assert.Equal(
                    1,
                    session.ExecuteQuery<IOgmEntity>($"MATCH ()-[{s} {{EntityId:{rels[0].EntityId}}}]->() WHERE {s}.SourcePropertyName<>'' RETURN {s}")
                        .Count()
                    );

                mgr.DeleteRels(session, rels);

                Assert.Equal(
                    0,
                    session.ExecuteQuery<IOgmEntity>($"MATCH ()-[{s} {{EntityId:{rels[0].EntityId}}}]->() RETURN {s}")
                        .Count()
                    );

                mgr.DeleteNodes(session, res);

                Assert.Equal(
                    0,
                    session.ExecuteQuery<IOgmEntity>($"MATCH {new Node(s, type: typeof(IOgmEntity), props: new { Name = name }.ToPropDictionary()).BuildForQuery()} RETURN {s}")
                        .Count()
                    );
            }
        }
    }
}
