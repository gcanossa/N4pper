using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class Chapter : EditableEntityBase
    {
        public string Name { get; set; }
        public Book Book { get; set; }
        public List<Section> Sections { get; set; }
    }
}
