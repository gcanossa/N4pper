//using System;
//using N4pper.Cypher;
//using System.Collections.Generic;
//using System.Text;
//using Xunit;
//using System.Text.RegularExpressions;
//using N4pper.Orm;
//using Neo4j.Driver.V1;
//using Newtonsoft.Json;

//namespace UnitTest
//{
//    public class CypherTests
//    {
//        #region nested types

//        public interface IFirst { }
//        public abstract class AClass : IFirst { }

//        public class ClassA : AClass
//        {
//            public int Id { get; set; }
//            public string Name { get; set; }
//            public DateTimeOffset? Time { get; set; }
//        }

//        #endregion

//        [Trait("Category", nameof(CypherTests))]
//        [Fact(DisplayName = nameof(Symbol))]
//        public void Symbol()
//        {
//            Symbol s = "s";
//            Assert.Equal("s", s);
//            Assert.Equal("prova", (Symbol)"prova");
//            Assert.Throws<ArgumentException>(() => (Symbol)"23M");
//        }

//        [Trait("Category", nameof(CypherTests))]
//        [Fact(DisplayName = nameof(Pattern))]
//        public void Pattern()
//        {
//            Assert.Equal("()--()", Cypr.Path()._._.ToString());
//            Assert.Equal("()-->()", Cypr.Path()._._X.ToString());
//            Assert.Equal("()<--()", Cypr.Path().X_._.ToString());

//            Assert.Equal("(p)<--()", Cypr.Path().Symbol("p").X_._.ToString());
//            Assert.Equal("(p)<-[*]-()", Cypr.Path().Symbol("p").X_.PathLength()._.ToString());
//            Assert.Equal("(p)<-[*1..]-()", Cypr.Path().Symbol("p").X_.PathLength(1)._.ToString());
//            Assert.Equal("(p)<-[*..2]-()", Cypr.Path().Symbol("p").X_.PathLength(null,2)._.ToString());
//            Assert.Equal("(p)<-[*1..3]-()", Cypr.Path().Symbol("p").X_.PathLength(1,3)._.ToString());
//            Assert.Equal("(p)<-[r*1..3]-()", Cypr.Path().Symbol("p").X_.PathLength(1, 3).Symbol("r")._.ToString());

//            Assert.Equal("(:test)<-[:prova]-()", Cypr.Path().SetLabels("test").X_.SetType("prova")._.ToString());

//            DateTimeOffset now = DateTimeOffset.Now;

//            ClassA c = new ClassA() { Id = 1, Name = "test", Time = now };

//            Assert.Equal(
//                $"(:_UnitTest$CypherTests$$ClassA:_UnitTest$CypherTests$$IFirst:_UnitTest$CypherTests$$AClass" +
//                $"{{Id:1,Name:\"test\",Time:{JsonConvert.SerializeObject(now)}}})<--()", 
//                Cypr.Path().Node<ClassA>(p=>new { p.Id, p.Name, p.Time }, c).X_._.ToString());
//            Assert.Equal(
//                $"(:_UnitTest$CypherTests$$ClassA:_UnitTest$CypherTests$$IFirst:_UnitTest$CypherTests$$AClass" +
//                $"{{Id:1,Name:\"test\",Time:{JsonConvert.SerializeObject(now)}}})<--()",
//                Cypr.Path().Node<ClassA>(c).X_._.ToString());
//        }

//        [Trait("Category", nameof(CypherTests))]
//        [Fact(DisplayName = nameof(PathClauses))]
//        public void PathClauses()
//        {
//            Assert.Equal("MATCH ()--()", Cypr.Match(Cypr.Path()._._).ToString());
//            Assert.Equal("OPTIONAL MATCH ()-->()", Cypr.OptionalMatch(Cypr.Path()._._X).ToString());

//            Assert.Equal("MATCH ()<--() SET p.Id=1,p.Test=\"ciao\"", Cypr.Match(Cypr.Path().X_._).Set("p", p=>p.Body(new { Id=1, Test="ciao" })).ToString());

//        }
//        [Trait("Category", nameof(CypherTests))]
//        [Fact(DisplayName = nameof(MergeClause))]
//        public void MergeClause()
//        {
//            Assert.Equal("MERGE ()<--() ON CREATE SET p.Id=1,p.Test=\"ciao\" ON MATCH SET p.Id=2", 
//                Cypr.Merge(Cypr.Path().X_._)
//                    .OnCreate("p", p => p.Body(new { Id = 1, Test = "ciao" }))
//                    .OnMatch("p", p => p.Body(new { Id = 2 })).ToString());

//        }
//    }
//}
