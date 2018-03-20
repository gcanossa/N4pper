using N4pper;
using Neo4j.Driver.V1;
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using AsIKnow.XUnitExtensions;
using N4pper.Orm;
using N4pper.Diagnostic;
using N4pper.QueryUtils;
using t=UnitTest.Types;
using UnitTest.Types;

namespace UnitTest
{
    [TestCaseOrderer(AsIKnow.XUnitExtensions.Constants.PriorityOrdererTypeName, AsIKnow.XUnitExtensions.Constants.PriorityOrdererTypeAssemblyName)]
    [Collection(nameof(Neo4jCollection))]
    public class OrmCoreTests
    {
        protected Neo4jFixture Fixture { get; set; }

        public OrmCoreTests(Neo4jFixture fixture)
        {
            Fixture = fixture;
        }

        public (IDriver, N4pperManager) SetUp()
        {
            return (
                Fixture.GetService<Neo4jFixture.TestContext>().Driver,
                Fixture.GetService<N4pperManager>()
                );
        }

        #region nested types

        public class PersonX
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public class Student : PersonX
        {
            public Teacher Teacher { get; set; }
        }

        public class Teacher : PersonX
        {
            public List<Student> Students { get; set; } = new List<Student>();
        }

        public class Class
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        public interface IEntity
        {
            long Id { get; }
        }
        public interface IContent : IEntity
        {

        }
        public interface IExercise : IContent
        {

        }
        public interface IExplaination : IContent
        {

        }
        public class Question : IExercise
        {
            public long Id { get; set; }
            public Student DoneBy { get; set; }
        }
        public class Suggestion : IExplaination
        {
            public long Id { get; set; }
            public Teacher GivenBy { get; set; }
        }

        public class ContentPersonRel : IEntity
        {
            public long Id { get; set; }
            public PersonX Person { get; set; }
            public IContent Content { get; set; }
        }

        public class EntityHolder : IEntity
        {
            public long Id { get; set; }
        }

        public class TestQueryTracer : IQueryTracer
        {
            public void Trace(string query)
            {
                Queries.Push(query);
            }

            public Stack<string> Queries { get; set; } = new Stack<string>();
        }

        #endregion

        [TestPriority(0)]
        [Trait("Category", nameof(OrmCoreTests))]
        [Fact(DisplayName = nameof(NodeCreation))]
        public void NodeCreation()
        {
            (IDriver driver, N4pperManager mgr) = SetUp();

            using (ISession session = driver.Session())
            {
                int count = session.Run($"MATCH (p) WHERE NOT p:{N4pper.Constants.GlobalIdentityNodeLabel} RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();

                PersonX p = session.AddOrUpdateNode<PersonX>(new PersonX() { Age = 1, Name = "pippy" });

                Assert.True(0 < p.Id);

                p.Age = 2;
                p = session.AddOrUpdateNode<PersonX>(p);

                Assert.Equal(2, p.Age);

                Assert.Equal(1, session.DeleteNode(p));

                int newcount = session.Run($"MATCH (p) WHERE NOT p:{N4pper.Constants.GlobalIdentityNodeLabel} RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();
                Assert.Equal(count, newcount);

                Assert.Equal(0, session.DeleteNode(p));
            }
        }

        [TestPriority(0)]
        [Trait("Category", nameof(OrmCoreTests))]
        [Fact(DisplayName = nameof(RelCreation))]
        public void RelCreation()
        {
            (IDriver driver, N4pperManager mgr) = SetUp();

            using (ISession session = driver.Session())
            {
                int count = session.Run($"MATCH ()-[p]-() RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();

                Student s1 = session.AddOrUpdateNode<Student>(new Student() { Age = 17, Name = "luca" });
                Student s2 = session.AddOrUpdateNode<Student>(new Student() { Age = 18, Name = "piero" });
                Student s3 = session.AddOrUpdateNode<Student>(new Student() { Age = 15, Name = "mario" });

                Teacher t1 = session.AddOrUpdateNode<Teacher>(new Teacher() { Age = 28, Name = "valentina" });

                Class c = session.AddOrUpdateRel<Class, Student, Teacher>(new Class() { Name = "3 A" }, s1, t1);

                Assert.True(0 < c.Id);

                c.Name = "3° A";
                c = session.AddOrUpdateRel<Class, Student, Teacher>(c);

                Assert.Equal("3° A", c.Name);

                Assert.Equal(1, session.DeleteRel<Class>(c));
                Assert.Equal(0, session.DeleteRel<Class>(c));

                int newcount = session.Run($"MATCH ()-[p]-() RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();
                Assert.Equal(count, newcount);
            }
        }

        [TestPriority(0)]
        [Trait("Category", nameof(OrmCoreTests))]
        [Fact(DisplayName = nameof(Query))]
        public void Query()
        {
            (IDriver driver, N4pperManager mgr) = SetUp();

            using (ISession session = driver.Session())
            {
                int count = session.Run($"MATCH ()-[p]-() RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();
                Assert.NotNull(QueryTraceLogger.LastStatement);

                Student s1 = session.AddOrUpdateNode<Student>(new Student() { Age = 17, Name = "luca" });
                Student s2 = session.AddOrUpdateNode<Student>(new Student() { Age = 18, Name = "piero" });
                Student s3 = session.AddOrUpdateNode<Student>(new Student() { Age = 15, Name = "mario" });

                Teacher t1 = session.AddOrUpdateNode<Teacher>(new Teacher() { Age = 28, Name = "valentina" });
                Teacher t2 = session.AddOrUpdateNode<Teacher>(new Teacher() { Age = 30, Name = "gianmaria" });

                Question[] qs = new Question[] { new Question(), new Question(), new Question(), new Question(), new Question() };

                session.WriteTransaction(tx =>
                {
                    qs = tx.AddOrUpdateNodes(qs).ToArray();
                });

                Suggestion[] ss = new Suggestion[] { new Suggestion(), new Suggestion() };

                session.WriteTransaction(tx =>
                {
                    ss = tx.AddOrUpdateNodes(ss).ToArray();
                });

                ContentPersonRel rel1 = session.AddOrUpdateRel(new ContentPersonRel(), s1, qs[0]);
                ContentPersonRel rel2 = session.AddOrUpdateRel(new ContentPersonRel(), s2, qs[1]);
                ContentPersonRel rel3 = session.AddOrUpdateRel(new ContentPersonRel(), s3, qs[0]);

                ContentPersonRel rel4 = session.AddOrUpdateRel(new ContentPersonRel(), t1, ss[0]);

                //TODO: la copia delle proprietà non funziona perché OMnG cerca le proprietà sul tipo dichiarato e non sull'effettivo.
                Symbol p = new Symbol();
                var tmp1 = session.ExecuteQuery<IContent>($"match {new Node(p, type:typeof(IContent))} return {p}").ToList();
                Assert.Equal(ss.Length + qs.Length, tmp1.Count());

                var tmp1_ = session.ExecuteQuery<IEntity>($"match {new Node(p, type: typeof(IEntity))} return {p}").ToList();
                Assert.Equal(ss.Length + qs.Length, tmp1_.Count());

                var tmp2 = session.ExecuteQuery<IEntity>($"match {new Node()._(p, typeof(ContentPersonRel))._V()} return {p}").ToList();
                Assert.Equal(4, tmp2.Count());
                
                session.WriteTransaction(tx =>
                {
                    tx.DeleteRel(rel1);
                    tx.DeleteRel(rel2);
                    tx.DeleteRel(rel3);
                    tx.DeleteRel(rel4);

                    tx.DeleteNode(s1);
                    tx.DeleteNode(s2);
                    tx.DeleteNode(s3);
                    tx.DeleteNode(t1);
                    tx.DeleteNode(t2);
                    tx.DeleteNodes(qs);
                    tx.DeleteNodes(ss);
                });

                int newcount = session.Run($"MATCH ()-[p]-() RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();
                Assert.Equal(count, newcount);
            }
        }

        [TestPriority(0)]
        [Trait("Category", nameof(OrmCoreTests))]
        [Fact(DisplayName = nameof(NodeLinking))]
        public void NodeLinking()
        {
            (IDriver driver, N4pperManager mgr) = SetUp();

            using (ISession session = driver.Session())
            {
                int count = session.Run($"MATCH ()-[p]-() RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();
                Assert.NotNull(QueryTraceLogger.LastStatement);
                
                t.Parent[] ps = new t.Parent[] { new t.Parent() { Name="Luca", Birthday = DateTime.Now }, new t.Parent() { Name = "Mario", Birthday = DateTime.Now } };

                session.WriteTransaction(tx =>
                {
                    ps = tx.AddOrUpdateNodes(ps).ToArray();
                });

                Assert.Equal(
                    0, 
                    session.ExecuteQuery<t.Person>(p => $"MATCH {p.Node<t.Person>(p.Symbol())}-{p.Rel<t.IRelatesTo>(p.Symbol())}->{p.Node<t.Person>(p.Symbol())} return {p.Symbols.First()}").Count());

                session.LinkNodes<t.Person, t.IRelatesTo, t.Person>(ps[0], ps[1]);

                Assert.Equal(
                    1,
                    session.ExecuteQuery<Person>(p => $"MATCH {p.Node<t.Person>(p.Symbol())}-{p.Rel<t.IRelatesTo>(p.Symbol())}->{p.Node<t.Person>(p.Symbol())} return {p.Symbols.First()}").Count());

                session.LinkNodes<t.Person, t.IRelatesTo, t.Person>(ps[1], ps[0]);
                session.LinkNodes<t.Person, t.IRelatesTo, t.Person>(ps[1], ps[0]);

                Assert.Equal(
                    2,
                    session.ExecuteQuery<Person>(p => $"MATCH {p.Node<t.Person>(p.Symbol())}-{p.Rel<t.IRelatesTo>(p.Symbol())}->{p.Node<t.Person>(p.Symbol())} return {p.Symbols.First()}").Count());

                List<Person> people = session.ExecuteQuery<Person>(
                    p => $"MATCH {p.Node<t.Person>(p.Symbol())}-{p.Rel<t.IRelatesTo>(p.Symbol())}->{p.Node<t.Person>(p.Symbol())} return {p.Symbols.First()} return {p.Symbols.First()}").ToList();

                session.UnlinkNodes<t.Person, t.IRelatesTo, t.Person>(ps[0], ps[1]);

                Assert.Equal(
                    1,
                    session.ExecuteQuery<Person>(p => $"MATCH {p.Node<t.Person>(p.Symbol())}-{p.Rel<t.IRelatesTo>(p.Symbol())}->{p.Node<t.Person>(p.Symbol())} return {p.Symbols.First()}").Count());

                session.UnlinkNodes<t.Person, t.IRelatesTo, t.Person>(ps[1], ps[0]);

                Assert.Equal(
                    0,
                    session.ExecuteQuery<Person>(p => $"MATCH {p.Node<t.Person>(p.Symbol())}-{p.Rel<t.IRelatesTo>(p.Symbol())}->{p.Node<t.Person>(p.Symbol())} return {p.Symbols.First()}").Count());

                session.WriteTransaction(tx =>
                {
                    tx.DeleteNodes(ps);
                });

                int newcount = session.Run($"MATCH ()-[p]-() RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();
                Assert.Equal(count, newcount);
            }
        }
    }
}
