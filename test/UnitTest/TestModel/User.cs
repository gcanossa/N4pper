using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime? Deathday { get; set; }
    }
}
