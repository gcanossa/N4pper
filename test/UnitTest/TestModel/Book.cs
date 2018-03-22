using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class Book : EditableEntityBase
    {
        public string Name { get; set; }
        public List<Chapter> Chapters { get; set; }
    }
}
