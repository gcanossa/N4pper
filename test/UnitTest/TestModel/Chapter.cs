using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class Chapter : EditableEntityBase
    {
        public virtual string Name { get; set; }
        public virtual Book Book { get; set; }
        public virtual ICollection<Section> Sections { get; set; }
    }
}
