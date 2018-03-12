using System;
using N4pper.Orm.Cypher;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Text.RegularExpressions;
using N4pper.Orm;
using Neo4j.Driver.V1;
using Newtonsoft.Json;

namespace UnitTest
{
    public class CyprTests
    {
        #region nested types

        public interface IFirst { }
        public abstract class AClass : IFirst { }

        public class ClassA : AClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTimeOffset? Time { get; set; }
        }

        #endregion
        
        [Trait("Category", nameof(CyprTests))]
        [Fact(DisplayName = nameof(Symbol))]
        public void Symbol()
        {
            Symbol s = Cypr.Symbol("s");
            Assert.Equal("s", s);
            Assert.Equal("prova", (Symbol)"prova");
            Assert.Throws<ArgumentException>(() => Cypr.Symbol("23M"));
            Assert.Matches("_[\\w]{32}", Cypr.Symbol());
        }

        [Trait("Category", nameof(CyprTests))]
        [Fact(DisplayName = nameof(Node))]
        public void Node()
        {
            Assert.Equal(
                "(test:_UnitTest$CyprTests$$ClassA:_UnitTest$CyprTests$$IFirst:_UnitTest$CyprTests$$AClass)", 
                Cypr.Node<ClassA>("test"));

            ClassA test = new ClassA() { Id=1, Name="pippo" };
            NodeExpressionBuilder<ClassA> node = Cypr.Node<ClassA>("test");
            node.WithBody().SetValues(test);

            Assert.Equal(
                "(test:_UnitTest$CyprTests$$ClassA:_UnitTest$CyprTests$$IFirst:_UnitTest$CyprTests$$AClass{Id:1,Name:\"pippo\",Time:null})",
                node);
        }
        [Trait("Category", nameof(CyprTests))]
        [Fact(DisplayName = nameof(Relationship))]
        public void Relationship()
        {
            Assert.Equal(
                "[test:_UnitTest$CyprTests$$ClassA]",
                Cypr.Rel<ClassA>("test"));

            ClassA test = new ClassA() { Id = 1, Name = "pippo" };
            RelationshipExpressionBuilder<ClassA> node = Cypr.Rel<ClassA>("test");
            node.WithBody().SetValues(test);

            Assert.Equal(
                "[test:_UnitTest$CyprTests$$ClassA{Id:1,Name:\"pippo\",Time:null}]",
                node);
        }

        [Trait("Category", nameof(CyprTests))]
        [Fact(DisplayName = nameof(Body))]
        public void Body()
        {
            EntityExpressionBodyBuilder<ClassA> body = new EntityExpressionBodyBuilder<ClassA>();
            DateTimeOffset now = DateTime.Now;
            ClassA ca = new ClassA() { Id = 1, Name = "pippo", Time = now };
            body.SetValues(ca);

            Assert.Equal($"{{Id:1,Name:\"pippo\",Time:{JsonConvert.SerializeObject(ca.Time)}}}", body);

            Dictionary<string, object> prs = body.Parametrize("_");

            Assert.Equal($"{{Id:$Id_,Name:$Name_,Time:$Time_}}", body);
            Assert.Equal(new Dictionary<string, object>() {
                { "Id_", ca.Id },
                { "Name_", ca.Name },
                { "Time_", ca.Time } },
                prs);
        }

        [Trait("Category", nameof(CyprTests))]
        [Fact(DisplayName = nameof(SetBody))]
        public void SetBody()
        {
            SetExpressionBodyBuilder<ClassA> body = new SetExpressionBodyBuilder<ClassA>();
            DateTimeOffset now = DateTime.Now;
            ClassA ca = new ClassA() { Id = 1, Name = "pippo", Time = now };
            body.SetValues(ca);

            Assert.Equal($"Id=1,Name=\"pippo\",Time={JsonConvert.SerializeObject(ca.Time)}", body);

            Dictionary<string, object> prs = body.Parametrize("_");

            Assert.Equal($"Id=$Id_,Name=$Name_,Time=$Time_", body);
            Assert.Equal(new Dictionary<string, object>() {
                { "Id_", ca.Id },
                { "Name_", ca.Name },
                { "Time_", ca.Time } },
                prs);

            body.ScopeProps(Cypr.Symbol("x"));

            Assert.Equal($"x.Id=$Id_,x.Name=$Name_,x.Time=$Time_", body);
        }
    }
}
