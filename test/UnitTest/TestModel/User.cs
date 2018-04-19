using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class User
    {
        public virtual long? EntityId { get; set; }
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime Birthday { get; set; }
        public virtual DateTime? Deathday { get; set; }
        public virtual TimeSpan Age { get { return (Deathday ?? DateTime.Now) - Birthday; } }
        public virtual ICollection<Friend> Friends { get; set; } = new List<Friend>();
        public virtual Friend BestFriend { get; set; }
        public virtual ICollection<IContent> OwnedContents { get; set; }
    }
}
