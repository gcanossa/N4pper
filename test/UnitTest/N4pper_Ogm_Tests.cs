using AsIKnow.XUnitExtensions;
using N4pper;
using N4pper.Ogm;
using N4pper.Ogm.Core;
using N4pper.Ogm.Entities;
using N4pper.QueryUtils;
using Neo4j.Driver.V1;
using OMnG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitTest.TestModel;
using Xunit;
using static UnitTest.Neo4jFixture;

namespace UnitTest
{
    [TestCaseOrderer(AsIKnow.XUnitExtensions.Constants.PriorityOrdererTypeName, AsIKnow.XUnitExtensions.Constants.PriorityOrdererTypeAssemblyName)]
    [Collection(nameof(Neo4jCollection))]
    public class N4pper_Ogm_Tests
    {
        protected Neo4jFixture Fixture { get; set; }

        public N4pper_Ogm_Tests(Neo4jFixture fixture)
        {
            Fixture = fixture;
        }

        [TestPriority(10)]
        [Trait("Category", nameof(N4pper_Ogm_Tests))]
        [Fact(DisplayName = nameof(CrUD_Nodes))]
        public void CrUD_Nodes()
        {
            using (GraphContext ctx = Fixture.GetService<GlobalTestContext>())
            {
                Symbol s = new Symbol();
                Book book = new Book() { Name = "Prova" };

                book = ctx.Add(book);

                book.Index = 1;

                Assert.Equal(
                    0,
                    ctx.Runner.ExecuteQuery<IOgmEntity>($"MATCH {new Node(s, type: typeof(Book), props: new { Name = "Prova" }.ToPropDictionary()).BuildForQuery()} RETURN {s}")
                        .Count()
                    );

                ctx.SaveChanges();

                Assert.Equal(
                    1,
                    ctx.Runner.ExecuteQuery<IOgmEntity>($"MATCH {new Node(s, type: typeof(Book), props: new { Name = "Prova" }.ToPropDictionary()).BuildForQuery()} RETURN {s}")
                        .Count()
                    );

                book.Name = "Prova 2";

                ctx.SaveChanges();
                Assert.Equal(
                    0,
                    ctx.Runner.ExecuteQuery<IOgmEntity>($"MATCH {new Node(s, type: typeof(Book), props: new { Name = "Prova" }.ToPropDictionary()).BuildForQuery()} RETURN {s}")
                        .Count()
                    );
                Assert.Equal(
                    1,
                    ctx.Runner.ExecuteQuery<IOgmEntity>($"MATCH {new Node(s, type: typeof(Book), props: new { Name = "Prova 2" }.ToPropDictionary()).BuildForQuery()} RETURN {s}")
                        .Count()
                    );

                ctx.Remove(book);

                ctx.SaveChanges();
                Assert.Equal(
                    0,
                    ctx.Runner.ExecuteQuery<IOgmEntity>($"MATCH {new Node(s, type: typeof(Book), props: new { Name = "Prova 2" }.ToPropDictionary()).BuildForQuery()} RETURN {s}")
                        .Count()
                    );
            }
        }
    }
}
