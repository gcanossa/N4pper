using Castle.Components.DictionaryAdapter;
using Castle.DynamicProxy;
using N4pper.Ogm.Core;
using N4pper.Ogm.Entities;
using OMnG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnitTest.TestModel;
using Xunit;

namespace UnitTest
{
    public class ChangeTracker_Tests
    {
        public class Connection : IOgmConnection
        {
            public virtual IOgmEntity Source { get; set; }
            public virtual IOgmEntity Destination { get; set; }
            public virtual string SourcePropertyName { get; set; }
            public virtual string DestinationPropertyName { get; set; }
            public virtual int Order { get; set; }
            public virtual long Version { get; set; }
            public virtual long? EntityId { get; set; }
        }

        public static IEnumerable<object[]> GetChangeTrackers()
        {
            yield return new[] { new DefaultChangeTracker() };
        }

        [Trait("Category", nameof(ChangeTracker_Tests))]
        [Theory(DisplayName = nameof(NodeCreation))]
        [MemberData(nameof(GetChangeTrackers))]
        public void NodeCreation(ChangeTrackerBase tracker)
        {
            Assert.Empty(tracker.GetChangeLog());

            Book book = new Book();
            Book book2 = new Book();
            Book book3 = new Book() { EntityId = 1 };

            tracker.Track(new EntityChangeNodeCreation(book));
            Assert.Equal(1, tracker.GetChangeLog().Count());
            tracker.Track(new EntityChangeNodeCreation(book));
            Assert.Equal(1, tracker.GetChangeLog().Count());
            tracker.Track(new EntityChangeNodeCreation(book2));
            Assert.Equal(2, tracker.GetChangeLog().Count());

            tracker.Track(new EntityChangeNodeDeletion(book3));
            Assert.Equal(3, tracker.GetChangeLog().Count());

            Assert.Throws<InvalidOperationException>(()=> tracker.Track(new EntityChangeNodeCreation(book3)));
            Assert.Equal(3, tracker.GetChangeLog().Count());
        }

        [Trait("Category", nameof(ChangeTracker_Tests))]
        [Theory(DisplayName = nameof(NodeDeletion))]
        [MemberData(nameof(GetChangeTrackers))]
        public void NodeDeletion(ChangeTrackerBase tracker)
        {
            Assert.Empty(tracker.GetChangeLog());

            Book book = new Book();
            Book book2 = new Book() { EntityId = 1 };
            Book book3 = new Book();

            tracker.Track(new EntityChangeNodeCreation(book));
            tracker.Track(new EntityChangeNodeUpdate(book2, typeof(Book).GetProperty(nameof(Book.Name)), book2.Name, book2.Name = "pippo"));
            Assert.Equal(2, tracker.GetChangeLog().Count());

            tracker.Track(new EntityChangeNodeDeletion(book2));
            Assert.Equal(2, tracker.GetChangeLog().Count());
            Assert.Equal(1, tracker.GetChangeLog().Where(p=>p is EntityChangeNodeDeletion).Count());

            tracker.Track(new EntityChangeNodeDeletion(book));
            Assert.Equal(1, tracker.GetChangeLog().Count());

            Assert.Throws<InvalidOperationException>(() => tracker.Track(new EntityChangeNodeDeletion(book3)));
        }

        [Trait("Category", nameof(ChangeTracker_Tests))]
        [Theory(DisplayName = nameof(NodeUpdate))]
        [MemberData(nameof(GetChangeTrackers))]
        public void NodeUpdate(ChangeTrackerBase tracker)
        {
            Assert.Empty(tracker.GetChangeLog());

            Book book = new Book();
            Book book2 = new Book() { EntityId = 1 };
            Book book3 = new Book();

            tracker.Track(new EntityChangeNodeCreation(book));
            tracker.Track(new EntityChangeNodeUpdate(book2, typeof(Book).GetProperty(nameof(Book.Name)), book2.Name, book2.Name = "pippo"));
            Assert.Equal(2, tracker.GetChangeLog().Count());
            tracker.Track(new EntityChangeNodeUpdate(book2, typeof(Book).GetProperty(nameof(Book.Name)), book2.Name, null));
            Assert.Equal(2, tracker.GetChangeLog().Count());

            tracker.Track(new EntityChangeNodeUpdate(book, typeof(Book).GetProperty(nameof(Book.Name)), book.Name, book.Name = "pippo"));
            Assert.Equal(2, tracker.GetChangeLog().Count());
            tracker.Track(new EntityChangeNodeUpdate(book2, typeof(Book).GetProperty(nameof(Book.Name)), book2.Name, book2.Name = "pippox"));
            Assert.Equal(2, tracker.GetChangeLog().Count());
            tracker.Track(new EntityChangeNodeUpdate(book2, typeof(Book).GetProperty(nameof(Book.Name)), book2.Name, book2.Name = "luca"));
            Assert.Equal(2, tracker.GetChangeLog().Count());
            Assert.Equal(1, tracker.GetChangeLog().Where(p => p.Entity == book2 && (p as EntityChangeNodeUpdate)?.CurrentValue == "luca").Count());
            Assert.Equal(0, tracker.GetChangeLog().Where(p => p.Entity == book2 && (p as EntityChangeNodeUpdate)?.CurrentValue == "pippox").Count());

            tracker.Track(new EntityChangeNodeUpdate(book2, typeof(Book).GetProperty(nameof(Book.Index)), book2.Index, book2.Index = 3));
            Assert.Equal(3, tracker.GetChangeLog().Count());

            Assert.Throws<ArgumentException>(() => tracker.Track(new EntityChangeNodeUpdate(book, typeof(Book).GetProperty(nameof(Book.Name)), book.Index, "pippo")));
            Assert.Throws<InvalidOperationException>(() => tracker.Track(new EntityChangeNodeUpdate(book3, typeof(Book).GetProperty(nameof(Book.Name)), book.Name, book.Name = "pippo")));
        }

        [Trait("Category", nameof(ChangeTracker_Tests))]
        [Theory(DisplayName = nameof(RelCreation))]
        [MemberData(nameof(GetChangeTrackers))]
        public void RelCreation(ChangeTrackerBase tracker)
        {
            Assert.Empty(tracker.GetChangeLog());

            Book book = new Book();
            Book book2 = new Book();
            Book book3 = new Book() { EntityId = 1 };
            Book book4 = new Book();
            Book book5 = new Book() { EntityId = 2 };
            Connection rel = new Connection();
            Connection rel2 = new Connection() { EntityId = 1 };
            Connection rel3 = new Connection();

            tracker.Track(new EntityChangeNodeCreation(book));
            tracker.Track(new EntityChangeNodeCreation(book2));
            tracker.Track(new EntityChangeNodeDeletion(book5));
            Assert.Equal(3, tracker.GetChangeLog().Count());

            tracker.Track(new EntityChangeRelCreation(rel, book, book3));
            Assert.Equal(4, tracker.GetChangeLog().Count());
            tracker.Track(new EntityChangeRelCreation(rel, book, book3));
            Assert.Equal(4, tracker.GetChangeLog().Count());
            tracker.Track(new EntityChangeRelUpdate(rel2, typeof(Connection).GetProperty(nameof(Connection.SourcePropertyName)), rel2.SourcePropertyName, rel2.SourcePropertyName="pippo"));
            Assert.Equal(5, tracker.GetChangeLog().Count());

            Assert.Throws<ArgumentException>(()=> tracker.Track(new EntityChangeRelCreation(rel3, book4, book3)));
            Assert.Throws<ArgumentException>(() => tracker.Track(new EntityChangeRelCreation(rel3, book, book4)));

            Assert.Throws<InvalidOperationException>(() => tracker.Track(new EntityChangeRelCreation(rel2, book, book3)));

            Assert.Throws<InvalidOperationException>(() => tracker.Track(new EntityChangeRelCreation(rel, book, book2)));

            Assert.Throws<ArgumentException>(() => tracker.Track(new EntityChangeRelCreation(rel3, book5, book3)));
        }

        [Trait("Category", nameof(ChangeTracker_Tests))]
        [Theory(DisplayName = nameof(RelDeletion))]
        [MemberData(nameof(GetChangeTrackers))]
        public void RelDeletion(ChangeTrackerBase tracker)
        {
            Assert.Empty(tracker.GetChangeLog());

            Book book = new Book();
            Book book2 = new Book();
            Book book3 = new Book() { EntityId = 1 };
            Book book4 = new Book();
            Connection rel = new Connection();
            Connection rel2 = new Connection() { EntityId = 1 };
            Connection rel3 = new Connection();

            tracker.Track(new EntityChangeNodeCreation(book));
            tracker.Track(new EntityChangeNodeCreation(book2));
            Assert.Equal(2, tracker.GetChangeLog().Count());

            tracker.Track(new EntityChangeRelCreation(rel, book, book3));
            tracker.Track(new EntityChangeRelUpdate(rel2, typeof(Connection).GetProperty(nameof(Connection.SourcePropertyName)), rel2.SourcePropertyName, rel2.SourcePropertyName = "pippo"));
            Assert.Equal(4, tracker.GetChangeLog().Count());

            tracker.Track(new EntityChangeRelDeletion(rel2));
            Assert.Equal(4, tracker.GetChangeLog().Count());
            Assert.Equal(1, tracker.GetChangeLog().Where(p => p is EntityChangeRelDeletion).Count());

            tracker.Track(new EntityChangeRelDeletion(rel));
            Assert.Equal(3, tracker.GetChangeLog().Count());

            tracker.Track(new EntityChangeRelDeletion(rel3));
            Assert.Equal(3, tracker.GetChangeLog().Count());

            tracker.Track(new EntityChangeRelCreation(rel3, book, book3));
            Assert.Equal(4, tracker.GetChangeLog().Count());
            tracker.Track(new EntityChangeNodeDeletion(book));
            Assert.Equal(2, tracker.GetChangeLog().Count());
        }

        [Trait("Category", nameof(ChangeTracker_Tests))]
        [Theory(DisplayName = nameof(RelUpdate))]
        [MemberData(nameof(GetChangeTrackers))]
        public void RelUpdate(ChangeTrackerBase tracker)
        {
            Assert.Empty(tracker.GetChangeLog());

            Book book = new Book();
            Book book2 = new Book();
            Book book3 = new Book() { EntityId = 1 };
            Book book4 = new Book();
            Connection rel = new Connection();
            Connection rel2 = new Connection() { EntityId = 1 };
            Connection rel3 = new Connection();

            tracker.Track(new EntityChangeNodeCreation(book));
            tracker.Track(new EntityChangeNodeCreation(book2));

            tracker.Track(new EntityChangeRelCreation(rel, book, book3));
            tracker.Track(new EntityChangeRelUpdate(rel2, typeof(Connection).GetProperty(nameof(Connection.SourcePropertyName)), rel2.SourcePropertyName, rel2.SourcePropertyName = "pippo"));
            Assert.Equal(4, tracker.GetChangeLog().Count());

            tracker.Track(new EntityChangeRelUpdate(rel2, typeof(Connection).GetProperty(nameof(Connection.SourcePropertyName)), rel2.SourcePropertyName, null));
            Assert.Equal(4, tracker.GetChangeLog().Count());

            tracker.Track(new EntityChangeRelUpdate(rel, typeof(Connection).GetProperty(nameof(Connection.SourcePropertyName)), rel.SourcePropertyName, rel.SourcePropertyName = "pippo"));
            Assert.Equal(4, tracker.GetChangeLog().Count());
            tracker.Track(new EntityChangeRelUpdate(rel2, typeof(Connection).GetProperty(nameof(Connection.SourcePropertyName)), rel2.SourcePropertyName, rel2.SourcePropertyName = "pippox"));
            Assert.Equal(4, tracker.GetChangeLog().Count());
            tracker.Track(new EntityChangeRelUpdate(rel2, typeof(Connection).GetProperty(nameof(Connection.SourcePropertyName)), rel2.SourcePropertyName, rel2.SourcePropertyName = "luca"));
            Assert.Equal(4, tracker.GetChangeLog().Count());
            Assert.Equal(1, tracker.GetChangeLog().Where(p => p.Entity == rel2 && (p as EntityChangeRelUpdate)?.CurrentValue == "luca").Count());
            Assert.Equal(0, tracker.GetChangeLog().Where(p => p.Entity == rel2 && (p as EntityChangeRelUpdate)?.CurrentValue == "pippox").Count());

            tracker.Track(new EntityChangeRelUpdate(rel2, typeof(Connection).GetProperty(nameof(Connection.Version)), rel2.Version, rel2.Version = 1));
            Assert.Equal(5, tracker.GetChangeLog().Count());

            Assert.Throws<ArgumentException>(() => tracker.Track(new EntityChangeRelUpdate(rel, typeof(Connection).GetProperty(nameof(Connection.Version)), rel.Version, "pippo")));
            Assert.Throws<InvalidOperationException>(() => tracker.Track(new EntityChangeRelUpdate(rel3, typeof(Connection).GetProperty(nameof(Connection.Version)), rel.Version, rel.Version = 1)));
        }


        [Trait("Category", nameof(ChangeTracker_Tests))]
        [Theory(DisplayName = nameof(RelMerge))]
        [MemberData(nameof(GetChangeTrackers))]
        public void RelMerge(ChangeTrackerBase tracker)
        {
            Assert.Empty(tracker.GetChangeLog());

            Book book = new Book();
            Book book2 = new Book();
            Book book3 = new Book() { EntityId = 1 };
            Book book4 = new Book();
            Book book5 = new Book() { EntityId = 2 };
            Connection rel = new Connection();
            Connection rel2 = new Connection() { EntityId = 1 };
            Connection rel3 = new Connection();

            tracker.Track(new EntityChangeNodeCreation(book));
            tracker.Track(new EntityChangeNodeCreation(book2));
            tracker.Track(new EntityChangeNodeDeletion(book5));
            Assert.Equal(3, tracker.GetChangeLog().Count());

            tracker.Track(new EntityChangeConnectionMerge(rel, book, book3, 0));
            Assert.Equal(4, tracker.GetChangeLog().Count());
            tracker.Track(new EntityChangeConnectionMerge(rel, book, book3, 1));
            Assert.Equal(4, tracker.GetChangeLog().Count());
            tracker.Track(new EntityChangeRelUpdate(rel, typeof(Connection).GetProperty(nameof(Connection.SourcePropertyName)), rel2.SourcePropertyName, rel2.SourcePropertyName = "pippo"));
            Assert.Equal(4, tracker.GetChangeLog().Count());

            tracker.Track(new EntityChangeConnectionMerge(rel, book, book2, 0));
            Assert.Equal(4, tracker.GetChangeLog().Count());
        }
    }
}
