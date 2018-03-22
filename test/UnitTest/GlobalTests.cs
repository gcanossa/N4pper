using AsIKnow.XUnitExtensions;
using N4pper;
using N4pper.Orm;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitTest.TestModel;
using Xunit;

namespace UnitTest
{
    [TestCaseOrderer(AsIKnow.XUnitExtensions.Constants.PriorityOrdererTypeName, AsIKnow.XUnitExtensions.Constants.PriorityOrdererTypeAssemblyName)]
    [Collection(nameof(Neo4jCollection))]
    public class GlobalTests
    {
        protected Neo4jFixture Fixture { get; set; }

        public GlobalTests(Neo4jFixture fixture)
        {
            Fixture = fixture;
        }

        private (IDriver, N4pperManager) SetUp()
        {
            return (
                Fixture.GetService<Neo4jFixture.GlobalTestContext>().Driver,
                Fixture.GetService<N4pperManager>()
                );
        }

        private int GetEntityNodesCount(ISession session)
        {
            return session.Run($"MATCH (p) WHERE NOT p:{N4pper.Constants.GlobalIdentityNodeLabel} RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();
        }

        private void TestBody(Action<ISession> body)
        {
            (IDriver driver, N4pperManager mgr) = SetUp();

            using (ISession session = driver.Session())
            {
                int count = GetEntityNodesCount(session);
                try
                {
                    body(session);
                }
                finally
                {
                    Assert.Equal(count, GetEntityNodesCount(session));
                }
            }
        }

        [TestPriority(0)]
        [Trait("Category", nameof(GlobalTests))]
        [Fact(DisplayName = nameof(NodeCreation))]
        public void NodeCreation()
        {
            TestBody(session=> 
            {
                var book = session.AddOrUpdateNode(new Book { Name = "Dune", Index=0 });
                var chapter1 = session.AddOrUpdateNode(new Chapter { Name = "Capitolo 1", Index = 0 });
                var chapter2 = session.AddOrUpdateNode(new Chapter { Name = "Capitolo 2", Index = 1 });

                session.LinkNodes<Book,Links.RelatesTo, Chapter>(book, chapter1);
                session.LinkNodes<Book, Links.RelatesTo, Chapter>(book, chapter2);

                IEnumerable<Book> tmp = session
                .ExecuteQuery<Book, IEnumerable<Chapter>>(
                    p=> $"match {p.Node<Book>(p.Symbol("p"))._(p.Rel<Links.RelatesTo>(p.Symbol()))._V(p.Node<Chapter>(p.Symbol("q")))} return p, collect(q)",
                    (b, c)=>
                    {
                        b.Chapters = new List<Chapter>();

                        b.Chapters.AddRange(c);
                        foreach (Chapter item in c)
                        {
                            item.Book = b;
                        }

                        return b;
                    });

                Assert.Equal(1, tmp.Count());
                Assert.Equal(2, tmp.First().Chapters.Count());
                Assert.Equal(tmp.First(), tmp.First().Chapters.First().Book);

                session.DeleteNode(chapter2);
                session.DeleteNode(chapter1);
                session.DeleteNode(book);
            });
        }
    }
}
