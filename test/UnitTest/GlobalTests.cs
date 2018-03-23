using AsIKnow.XUnitExtensions;
using N4pper;
using N4pper.Orm;
using N4pper.Orm.Entities;
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

        private GraphContext SetUp()
        {
            return 
                Fixture.GetService<Neo4jFixture.GlobalTestContext>()
                ;
        }

        private int GetEntityNodesCount(ISession session)
        {
            return session.Run($"MATCH (p) WHERE NOT p:{N4pper.Constants.GlobalIdentityNodeLabel} RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();
        }

        private void TestBody(Action<ISession, GraphContext> body)
        {
            GraphContext ctx = SetUp();

            using (ISession session = ctx.Driver.Session())
            {
                int count = GetEntityNodesCount(session);
                try
                {
                    body(session, ctx);
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
            TestBody((session, ctx)=> 
            {
                var book = session.AddOrUpdateNode(new Book { Name = "Dune", Index=0 });
                var chapter1 = session.AddOrUpdateNode(new Chapter { Name = "Capitolo 1", Index = 0 });
                var chapter2 = session.AddOrUpdateNode(new Chapter { Name = "Capitolo 2", Index = 1 });

                session.LinkNodes<Links.RelatesTo, Book, Chapter>(book, chapter1);
                session.LinkNodes<Links.RelatesTo, Book, Chapter>(book, chapter2);

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
                Assert.Equal(tmp.First().Id, tmp.First().Chapters.First().Book.Id);

                session.DeleteNode(chapter2);
                session.DeleteNode(chapter1);
                session.DeleteNode(book);
            });
        }

        [TestPriority(0)]
        [Trait("Category", nameof(GlobalTests))]
        [Fact(DisplayName = nameof(ManagedCreation))]
        public void ManagedCreation()
        {
            TestBody((session, ctx) =>
            {
                var user = new User() { Birthday = new DateTime(1988, 1, 30), Name = "Gianmaria" };
                var user2 = new User() { Birthday = new DateTime(1989, 9, 28), Name = "Valentina" };
                var book = new Book { Name = "Dune", Index = 0, Owner = user, Contributors = new List<User>() { user, user2 } };
                var chapter1 = new Chapter { Name = "Capitolo 1", Index = 0, Book = book, Owner = user, Contributors = new List<User>() { user, user2 } };
                var chapter2 = new Chapter { Name = "Capitolo 2", Index = 1, Book = book, Owner = user, Contributors = new List<User>() { user, user2 } };
                book.Chapters = new List<Chapter>() {chapter1, chapter2 };

                ctx.Add(user);
                ctx.Add(chapter2);

                ctx.SaveChanges(session);

                //var chapter3 = new Chapter { Name = "Capitolo 3", Index = 1, Book = book };
                //book.Chapters = new List<Chapter>() { chapter1, chapter3 };

                //ctx.SaveChanges(session);

                Book bookQ = ctx.Query<Book>(session, k=> 
                {
                    k.Include(p => p.Chapters).Include(p => p.Contributors);
                    k.Include(p => p.Chapters).Include(p => p.Owner);
                    k.Include(p => p.Contributors);
                    k.Include(p => p.Owner);
                }).First(p=>p.Id>0);

                IEnumerable<Book> tmp = session
                .ExecuteQuery<Book, IEnumerable<Chapter>>(
                    p => $"match {p.Node<Book>(p.Symbol("p"))._(p.Rel<Connection>(p.Symbol()))._V(p.Node<Chapter>(p.Symbol("q")))} return p, reverse(collect(q))",
                    (b, c) =>
                    {
                        b.Chapters = new List<Chapter>();

                        b.Chapters.AddRange(c);
                        foreach (Chapter item in c)
                        {
                            item.Book = b;
                        }

                        return b;
                    }).ToList();

                Assert.Equal(1, tmp.Count());
                Assert.Equal(2, tmp.First().Chapters.Count());
                Assert.Equal(tmp.First().Id, tmp.First().Chapters.First().Book.Id);

                //ctx.Remove(chapter3);
                ctx.Remove(chapter2);
                ctx.Remove(chapter1);
                ctx.Remove(book);

                ctx.SaveChanges(session);
            });
        }
    }
}
