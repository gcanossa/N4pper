using N4pper;
using Neo4j.Driver.V1;
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using AsIKnow.Graph;
using AsIKnow.XUnitExtensions;
using N4pper.Orm;
using N4pper.Diagnostic;

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
                Fixture.GetService<IDriver>(),
                Fixture.GetService<N4pperManager>()
                );
        }

        #region nested types

        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public class Student : Person
        {
            public Teacher Teacher { get; set; }
        }

        public class Teacher : Person
        {
            public List<Student> Students { get; set; } = new List<Student>();
        }

        public class Class
        {
            public int Id { get; set; }
            public string Name { get; set; }
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
            
            using (ISession session = driver.Session().WithGraphManager(mgr))
            {
                int count = session.Run($"MATCH (p) WHERE NOT p:{StatementHelpers.GlobalIdentityNodeLabel} RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();

                Person p = session.AddOrUpdateNode<Person>(new Person() { Age=1, Name="pippy" });

                Assert.True(0 < p.Id);

                p.Age = 2;
                p = session.AddOrUpdateNode<Person>(p);

                Assert.Equal(2, p.Age);

                Assert.Equal(1, session.DeleteNode(p));
                
                int newcount = session.Run($"MATCH (p) WHERE NOT p:{StatementHelpers.GlobalIdentityNodeLabel} RETURN COUNT(p)").Select(x=>x.Values[x.Keys[0]].As<int>()).First();
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
            
            using (ISession session = driver.Session().WithGraphManager(mgr))
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
                
                Assert.Equal(1,session.DeleteRel<Class>(c));
                Assert.Equal(0, session.DeleteRel<Class>(c));

                int newcount = session.Run($"MATCH ()-[p]-() RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();
                Assert.Equal(count, newcount);
            }
        }
    }
}
