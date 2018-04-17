using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using N4pper.QueryUtils;
using OMnG;
using UnitTest.Types;
using Castle.Components.DictionaryAdapter;

namespace UnitTest
{
    public class QueryUtilsTests
    {
        #region nested types

        public abstract class Valued<T>
        {
            public virtual T Value { get; set; }
        }

        public class TestDateTime : Valued<int>
        {
            public override int Value { get; set; }
            public string ValueString { get; set; }
            public DateTime ValueDate { get; set; }
            public DateTimeOffset ValueDateOff { get; set; }
            public DateTime? ValueDateNull { get; set; }
            public DateTimeOffset? ValueDateOffNull { get; set; }
        }

        public class TestTimeSpan
        {
            public TimeSpan TimeSpan { get; set; }
            public TimeSpan? TimeSpanNull { get; set; }
        }

        #endregion

        [Trait("Category", "QueryUtilsTests")]
        [Fact(DisplayName = nameof(DateTime_Test))]
        public void DateTime_Test()
        {
            using (ObjectExtensions.ConfigScope(new N4pper.ObjectExtensionsConfiguration()))
            {
                DateTime now = DateTime.Now;
                now = now.AddTicks(-(now.Ticks % TimeSpan.FromMilliseconds(1).Ticks));
                TestDateTime test = new TestDateTime() { Value = 1, ValueString = "test", ValueDate = now, ValueDateOff = now, ValueDateNull = now, ValueDateOffNull = now };

                var props = test.ToPropDictionary();

                props["ValueDate"] = ((DateTimeOffset)now).ToUnixTimeMilliseconds();
                props["ValueDateOff"] = ((DateTimeOffset)now).ToUnixTimeMilliseconds();
                props["ValueDateNull"] = ((DateTimeOffset)now).ToUnixTimeMilliseconds();
                props["ValueDateOffNull"] = ((DateTimeOffset)now).ToUnixTimeMilliseconds();

                test = test.CopyProperties(props);

                Assert.Equal(now, test.ValueDate);
                Assert.Equal(now, test.ValueDateOff);
                Assert.Equal(now, test.ValueDateNull);
                Assert.Equal(now, test.ValueDateOffNull);
            }
        }

        [Trait("Category", "QueryUtilsTests")]
        [Fact(DisplayName = nameof(TimeSpan_Test))]
        public void TimeSpan_Test()
        {
            using (ObjectExtensions.ConfigScope(new N4pper.ObjectExtensionsConfiguration()))
            {
                TestTimeSpan test = new TestTimeSpan() { TimeSpan = TimeSpan.FromMilliseconds(1234), TimeSpanNull = TimeSpan.FromMilliseconds(1234) };

                var props = test.ToPropDictionary();

                props["TimeSpan"] = 1234;
                props["TimeSpanNull"] = 1234;

                test = test.CopyProperties(props);

                Assert.Equal(TimeSpan.FromMilliseconds(1234), test.TimeSpan);
                Assert.Equal(TimeSpan.FromMilliseconds(1234), test.TimeSpanNull);
            }
        }

        [Trait("Category", nameof(QueryUtilsTests))]
        [Fact(DisplayName = nameof(Syntax))]
        public void Syntax()
        {
            Assert.Equal("()", new Node());
            Assert.Equal("[]", new Rel());

            Assert.Equal("(p)", new Node("p"));
            Assert.Equal("[r]", new Rel("r"));

            Assert.Equal("$test", (Parameter)"test");

            Assert.Equal("(p:`UnitTest.Types.Child`:`UnitTest.Types.IUniqueId`:`UnitTest.Types.IMortal`:`UnitTest.Types.Person`)", new Node("p", typeof(Child)));
            Assert.Equal("[r:`UnitTest.Types.Child`]", new Rel("r", typeof(Child)));

            Assert.Equal("(p:`UnitTest.Types.Child`:`UnitTest.Types.IUniqueId`:`UnitTest.Types.IMortal`:`UnitTest.Types.Person`{Id:3})", new Node<Child>("p", new { Id = 3 }));
            Assert.Equal("[r:`UnitTest.Types.Child`{Id:3}]", new Rel<Child>("r", new { Id=3 }));

            DateTime now = DateTime.Now.TruncateDateToTimeslice(TimeSpan.FromMilliseconds(1));
            Assert.Equal($"(p:`UnitTest.Types.Child`:`UnitTest.Types.IUniqueId`:`UnitTest.Types.IMortal`:`UnitTest.Types.Person`{{Id:1,Name:'Pippo',Birthday:{((DateTimeOffset)now).ToUnixTimeMilliseconds()},Deathday:null}})", new Node("p", typeof(Child), new Child() { Id=1, Name="Pippo", Birthday = now }.SelectPrimitiveTypesProperties()));
            Assert.Equal($"[r:`UnitTest.Types.Child`{{Id:1,Name:'Pippo',Birthday:{((DateTimeOffset)now).ToUnixTimeMilliseconds()},Deathday:null}}]", new Rel("r", typeof(Child), new Child() { Id = 1, Name = "Pippo", Birthday = now }.SelectPrimitiveTypesProperties()));

            Assert.Equal("()-[]-()<-[r:`UnitTest.Types.Child`]-(p{Name:'Luca'})-[]->(c)",
                new Node()._()._().V_("r",typeof(Child))._("p",null,new { Name="Luca" }.ToPropDictionary())._()._V("c")
                );

            Assert.Equal("()-[]-()<-[r:`UnitTest.Types.Child`]-(p{Name:'Luca'})-[]->(c)",
                new Node()._()._().V_().SetType(typeof(Child)).SetSymbol("r")._(type: null, props:new { Name = "Luca" }.ToPropDictionary()).SetSymbol("p")._()._V("c")
                );

            Assert.Equal($"Id=1,Name='Pippo',Birthday={((DateTimeOffset)now).ToUnixTimeMilliseconds()},Deathday=null", new Set(props: new Child() { Id = 1, Name = "Pippo", Birthday = now }.SelectPrimitiveTypesProperties()));
            Assert.Equal($"c.Id=1,c.Name='Pippo',c.Birthday={((DateTimeOffset)now).ToUnixTimeMilliseconds()},c.Deathday=null", new Set("c", new Child() { Id = 1, Name = "Pippo", Birthday = now }.SelectPrimitiveTypesProperties()));
            Assert.Equal($"c.Id=1,c.Name='Pippo',c.Birthday={((DateTimeOffset)now).ToUnixTimeMilliseconds()},c.Deathday=null", new Set(props:new Child() { Id = 1, Name = "Pippo", Birthday = now }.SelectPrimitiveTypesProperties()).SetSymbol("c"));

            Node n = new Node("p", typeof(Child), new Child() { Id = 1, Name = "Pippo", Birthday = now }.SelectPrimitiveTypesProperties());
            Node n1 = new Node("p", typeof(Child), new Child() { Id = 1, Name = "Pippo", Birthday = now }.SelectPrimitiveTypesProperties());
            Parameters p = n.Parametrize();
            Assert.Equal($"(p:`UnitTest.Types.Child`:`UnitTest.Types.IUniqueId`:`UnitTest.Types.IMortal`:`UnitTest.Types.Person`{{Id:$Id,Name:$Name,Birthday:{((DateTimeOffset)now).ToUnixTimeMilliseconds()},Deathday:$Deathday}})", n);

            n1.Parametrize(p);
            Assert.Equal($"(p:`UnitTest.Types.Child`:`UnitTest.Types.IUniqueId`:`UnitTest.Types.IMortal`:`UnitTest.Types.Person`{{Id:$Id,Name:$Name,Birthday:{((DateTimeOffset)now).ToUnixTimeMilliseconds()},Deathday:$Deathday}})", n1);

            Node x = new Node(type: typeof(TestDateTime));
            string y = x.ToString();
        }
    }
}
