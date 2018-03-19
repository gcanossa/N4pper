using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using N4pper.QueryUtils;
using OMnG;
using UnitTest.Types;

namespace UnitTest
{
    public class QueryUtilsTests
    {
        [Trait("Category", nameof(QueryUtilsTests))]
        [Fact(DisplayName = nameof(Syntax))]
        public void Syntax()
        {
            Assert.Equal("()", new Node());
            Assert.Equal("[]", new Rel());

            Assert.Equal("(p)", new Node("p"));
            Assert.Equal("[r]", new Rel("r"));

            Assert.Equal("(p:UnitTest.Types.Child:UnitTest.Types.IUniqueId:UnitTest.Types.IMortal:UnitTest.Types.Person)", new Node("p", typeof(Child)));
            Assert.Equal("[r:UnitTest.Types.Child]", new Rel("r", typeof(Child)));
            
            DateTime now = DateTime.Now.Truncate(TimeSpan.FromMilliseconds(1));
            Assert.Equal($"(p:UnitTest.Types.Child:UnitTest.Types.IUniqueId:UnitTest.Types.IMortal:UnitTest.Types.Person{{Id:1,Name:'Pippo',Birthday:{((DateTimeOffset)now).ToUnixTimeMilliseconds()},Deathday:null}})", new Node("p", typeof(Child), new Child() { Id=1, Name="Pippo", Birthday = now }.SelectPrimitiveTypesProperties()));
            Assert.Equal($"[r:UnitTest.Types.Child{{Id:1,Name:'Pippo',Birthday:{((DateTimeOffset)now).ToUnixTimeMilliseconds()},Deathday:null}}]", new Rel("r", typeof(Child), new Child() { Id = 1, Name = "Pippo", Birthday = now }.SelectPrimitiveTypesProperties()));

            Assert.Equal("()-[]-()<-[r:UnitTest.Types.Child]-(p{Name:'Luca'})-[]->(c)",
                new Node()._()._().V_("r",typeof(Child))._("p",null,new { Name="Luca" }.ToPropDictionary())._()._V("c")
                );

            Assert.Equal("()-[]-()<-[r:UnitTest.Types.Child]-(p{Name:'Luca'})-[]->(c)",
                new Node()._()._().V_().SetType(typeof(Child)).SetSymbol("r")._(type: null, props:new { Name = "Luca" }.ToPropDictionary()).SetSymbol("p")._()._V("c")
                );

            Assert.Equal($"Id=1,Name='Pippo',Birthday={((DateTimeOffset)now).ToUnixTimeMilliseconds()},Deathday=null", new Set(props: new Child() { Id = 1, Name = "Pippo", Birthday = now }.SelectPrimitiveTypesProperties()));
            Assert.Equal($"c.Id=1,c.Name='Pippo',c.Birthday={((DateTimeOffset)now).ToUnixTimeMilliseconds()},c.Deathday=null", new Set("c", new Child() { Id = 1, Name = "Pippo", Birthday = now }.SelectPrimitiveTypesProperties()));
            Assert.Equal($"c.Id=1,c.Name='Pippo',c.Birthday={((DateTimeOffset)now).ToUnixTimeMilliseconds()},c.Deathday=null", new Set(props:new Child() { Id = 1, Name = "Pippo", Birthday = now }.SelectPrimitiveTypesProperties()).SetSymbol("c"));

            Node n = new Node("p", typeof(Child), new Child() { Id = 1, Name = "Pippo", Birthday = now }.SelectPrimitiveTypesProperties());
            Node n1 = new Node("p", typeof(Child), new Child() { Id = 1, Name = "Pippo", Birthday = now }.SelectPrimitiveTypesProperties());
            Parameters p = n.Parametrize();
            Assert.Equal("(p:UnitTest.Types.Child:UnitTest.Types.IUniqueId:UnitTest.Types.IMortal:UnitTest.Types.Person{Id:$Id,Name:$Name,Birthday:$Birthday,Deathday:$Deathday})", n);

            n1.Parametrize(p);
            Assert.Equal("(p:UnitTest.Types.Child:UnitTest.Types.IUniqueId:UnitTest.Types.IMortal:UnitTest.Types.Person{Id:$Id,Name:$Name,Birthday:$Birthday,Deathday:$Deathday})", n1);
        }
    }
}
