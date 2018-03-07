using N4pper;
using Neo4j.Driver.V1;
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using AsIKnow.Graph;
using N4pper.Diagnostic;

namespace UnitTest
{
    public class UnitTest1
    {
        public (IDriver, GraphManager) SetUp()
        {
            return (
                GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j","test")),
                new GraphManager(new ReflectionTypeManager(new TypeManagerOptions()))
                );
        }

        #region nested types

        public interface IPositionable
        {
            int Position { get; set; }
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }

        }

        public class Runner : Person, IPositionable
        {
            public int Position { get; set; }

            public Person Friend { get; set; }
        }

        #endregion

        [Trait("Category", nameof(UnitTest1))]
        [Fact(DisplayName = nameof(TestDebug))]
        public void TestDebug()
        {
            (IDriver driver, GraphManager mgr) = SetUp();

            using (ISession session = driver.Session().WithGraphManager(mgr))
            {
                var result = session.Run("MATCH (p)<-[r]-(q) RETURN p,r,q");
                List<IRecord> records = result.AsEnumerable().ToList();
                
                result = session.Run("MATCH (p) RETURN p");
                records = result.AsEnumerable().ToList();
                
                result = session.Run("MATCH (p) RETURN p.Name");
                records = result.AsEnumerable().ToList();

                result = session.Run("MATCH (p:Runner) RETURN p");
                records = result.AsEnumerable().ToList();
            }
        }
    }
}
