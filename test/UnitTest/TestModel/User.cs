using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime? Deathday { get; set; }
        public TimeSpan Age { get { return (Deathday ?? DateTime.Now) - Birthday; } }
        public List<Friend> Friends { get; set; } = new List<Friend>();
    }
}
