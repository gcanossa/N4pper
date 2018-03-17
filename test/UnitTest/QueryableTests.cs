using N4pper;
using N4pper.Queryable;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Neo4j.Driver.V1;
using OMnG;

namespace UnitTest
{
    public class QueryableTests
    {
        #region nested types

        public interface IEntity
        {
            int Id { get; set; }
        }
        public abstract class Person : IEntity
        {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
            public DateTime Birthday { get; set; }
            public DateTime? Deathday { get; set; }
        }

        public class Parent : Person
        {
            public List<Child> Children { get; set; }
        }

        public class Child : Person
        {
            public Parent Parent { get; set; }
        }

        #endregion

        [Trait("Category", nameof(QueryableTests))]
        [Fact(DisplayName = nameof(Debug))]
        public void Debug()
        {
            IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.None);

            using (ISession session = driver.Session())
            {

                var query = session.ExecuteQuery<IEnumerable<Child>>(
                    "MATCH (q:`UnitTest.QueryableTests+Child`)-[:Of]->(x:`UnitTest.QueryableTests+Parent`) RETURN collect(q) as q,collect(x) as x",
                    new Dictionary<string, object>() { { "Id", "3" } });
                
                DateTime date = DateTime.Now - TimeSpan.FromDays(1);

                var test =
                    query.ToList();//.Select((p, i) => new { p.Id }).ToList();
                    //.Select(p=> new { p.Id, p.Name})
                    //.Select(p => new { p.Id })
                    //.Select((p, i)=> new { p.Id })
                    //.Select(p => p.Id).First(p=>p==2);
                
                //Assert.Equal(1,test.Count());
                //Assert.Equal(1, test.ToList().Count());

                //var test1 =
                //    query
                //    .Where(p => p.Birthday > date && p.Deathday == null)
                //    .Where(p => p.Id >= 1)
                //    .Where(p => p.Name.StartsWith("lui"))
                //    .Where(q => q.Name.EndsWith("jo")).OrderBy(p => p.Id)
                //    .Where(p => p.Name.Contains("xy")).OrderByDescending(p => p.Id).ThenBy(p => p.Name).ThenByDescending(p => p.Id)
                //    .Take(3).Take(2).Skip(2).Skip(1)
                //    .Select(p => new { p.Id, p.Name })
                //    .Distinct().Count(p => p.Id > 0);

                ////test.ToList();

                //var test2 =
                //    query
                //    .Where(p => p.Birthday > date && p.Deathday == null)
                //    .Where(p => p.Id >= 1)
                //    .Where(p => p.Name.StartsWith("lui"))
                //    .Where(p => p.Name.EndsWith("jo"))
                //    .Where(p => p.Name.Contains("xy"))
                //    .Distinct().First();
            }
        }
    }
}
