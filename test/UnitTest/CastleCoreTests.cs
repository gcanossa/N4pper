using Castle.Components.DictionaryAdapter;
using Castle.DynamicProxy;
using OMnG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Xunit;

namespace UnitTest
{
    public class CastleCoreTests
    {
        public interface IIdentity : IEditableObject, INotifyPropertyChanging, INotifyPropertyChanged
        {
            int Id { get; set; }
        }
        public interface IProva : IIdentity
        {
            string Name { get; set; }
            DateTime Birthdate { get; set; }
            DateTime? Deathdate { get; set; }
            TimeSpan Age { get; }
        }

        public class ProvaX : IProva
        {
            public int Id { get; set; }
            public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public DateTime Birthdate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public DateTime? Deathdate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public TimeSpan Age => throw new NotImplementedException();

            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;

            public void BeginEdit()
            {
                throw new NotImplementedException();
            }

            public void CancelEdit()
            {
                throw new NotImplementedException();
            }

            public void EndEdit()
            {
                throw new NotImplementedException();
            }
        }

        [Trait("Category", nameof(CastleCoreTests))]
        [Fact(DisplayName = nameof(Test))]
        public void Test()
        {
            DictionaryAdapterFactory factory = new DictionaryAdapterFactory();
            
            IDictionary<string, object> dic = new { Id = 3, Name = "Paolo", Birthdate = new DateTime(1988, 01, 12) }.ToPropDictionary();

            IProva prova = factory.GetAdapter<IProva, object>(dic);
            IIdentity identity = factory.GetAdapter<IIdentity, object>(dic);
            
            Assert.Equal(3, prova.Id);
            Assert.Equal(3, identity.Id);

            IEditableObject rev = prova as IEditableObject;
            INotifyPropertyChanging ci = prova as INotifyPropertyChanging;
            INotifyPropertyChanged ce = prova as INotifyPropertyChanged;

            ci.PropertyChanging += (s, e) => Console.WriteLine(e);
            ce.PropertyChanged += (s, e) => Console.WriteLine(e);

            rev.BeginEdit();

            prova.Id = 4;

            rev.CancelEdit();

            IDictionary<string, object> dic2 = prova.SelectProperties(typeof(IProva));
        }

        [Trait("Category", nameof(CastleCoreTests))]
        [Fact(DisplayName = nameof(Test2))]
        public void Test2()
        {
            ProvaX p = new ProvaX() { Id = 1 };

            int x;
            for (int i = 0; i < 1000000; i++)
            {
                x = p.Id;
            }
        }
        [Trait("Category", nameof(CastleCoreTests))]
        [Fact(DisplayName = nameof(Test3))]
        public void Test3()
        {
            ProvaX p = new ProvaX() { Id = 1 };

            PropertyInfo pinfo = p.GetType().GetProperty("Id");

            int x;
            for (int i = 0; i < 1000000; i++)
            {
                x = (int)pinfo.GetValue(p);
            }
        }
        [Trait("Category", nameof(CastleCoreTests))]
        [Fact(DisplayName = nameof(Test4))]
        public void Test4()
        {
            ProvaX p = new ProvaX() { Id = 1 };

            PropertyInfo pinfo = p.GetType().GetProperty("Id");

            Delegate f = Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(pinfo.ReflectedType, pinfo.PropertyType), null, pinfo.GetGetMethod());

            ParameterExpression par = Expression.Parameter(typeof(object));
            Func<object, object> fo = Expression.Lambda<Func<object, object>>(
                Expression.Convert(Expression.Invoke(Expression.Convert(Expression.Constant(f), typeof(Func<,>).MakeGenericType(pinfo.ReflectedType, pinfo.PropertyType)),Expression.Convert(par,typeof(ProvaX))), typeof(object)),
                par)
                .Compile();

            //Func<object, object> fo = (obj) => f((ProvaX)obj);
            int x;
            for (int i = 0; i < 1000000; i++)
            {
                x = (int)fo(p);
            }

            //PropertyInfo property = pinfo;

            //Delegate d = Delegate.CreateDelegate(
            //        typeof(Action<,>).MakeGenericType(property.ReflectedType, property.PropertyType),
            //        null,
            //        property.GetSetMethod());

            //ParameterExpression targetParam = Expression.Parameter(typeof(object));
            //ParameterExpression valueParam = Expression.Parameter(typeof(object));
            //Action<object, object> fox =
            //        Expression.Lambda<Action<object, object>>(
            //                Expression.Invoke(
            //                    Expression.Convert(
            //                        Expression.Constant(d),
            //                        typeof(Action<,>).MakeGenericType(property.ReflectedType, property.PropertyType)
            //                        ),
            //                    Expression.Convert(targetParam, property.ReflectedType),
            //                    Expression.Convert(valueParam, property.PropertyType)
            //                ),
            //        targetParam, valueParam)
            //        .Compile();

            //fox(p, 4);
        }

        [Trait("Category", nameof(CastleCoreTests))]
        [Fact(DisplayName = nameof(Test5))]
        public void Test5()
        {
            for (int i = 0; i < 1000000; i++)
            {
                Assert.Equal(5, typeof(ProvaX).GetInterfaces().Length);
                Assert.True(typeof(ProvaX).GetInterfaces().Contains(typeof(IProva)));
            }
        }
        [Trait("Category", nameof(CastleCoreTests))]
        [Fact(DisplayName = nameof(Test6))]
        public void Test6()
        {
            List<Type> tps = typeof(ProvaX).GetInterfaces().ToList();
            for (int i = 0; i < 1000000; i++)
            {
                Assert.Equal(5, tps.Count);
                Assert.True(tps.Contains(typeof(IProva)));
            }
        }

        public interface IEntity
        {
            int Id { get; set; }
        }

        public class Items
        {
            public int Id { get; set; }
            public virtual string Name { get; set; }
            public virtual IList<string> Titles { get; set; } = new List<string>();
        }

        public class Items2 : Items, IEntity
        {
            public new virtual int Id { get { return base.Id; } set { base.Id = value; } }
        }

        public class TestInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();
            }
        }

        [Trait("Category", nameof(CastleCoreTests))]
        [Fact(DisplayName = nameof(Test7))]
        public void Test7()
        {
            long value = 1;
            ParameterExpression p = Expression.Parameter(typeof(object));
            Func<object, object> converter = Expression.Lambda<Func<object, object>>(
                Expression.Convert(Expression.Convert(Expression.Convert(p, typeof(long)), typeof(int)), typeof(object)),
                p
                ).Compile();
            object xx = converter((object)value);

            Assert.Equal(typeof(int), xx.GetType());

            ProxyGenerator gen = new ProxyGenerator();
            Items x = new Items() { Id=1, Name="uno" };
            x.Titles.Add("aaa");
            x.Titles.Add("bbb");

            List<int> val = new List<int>();
            IList<int> val2 = (IList<int>)gen.CreateInterfaceProxyWithTarget(typeof(IList<int>), val, new TestInterceptor());
            val2.Add(1);
        }
    }
}
