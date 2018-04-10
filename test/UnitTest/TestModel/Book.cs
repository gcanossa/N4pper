using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class Book : EditableEntityBase
    {
        public virtual string Name { get; set; }
        public virtual List<Chapter> Chapters { get; set; }
    }
}
