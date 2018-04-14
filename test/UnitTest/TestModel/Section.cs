using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class Section : EditableEntityBase
    {
        public virtual Chapter Chapter { get; set; }
        public virtual ICollection<IContent> Contents { get; set; }
    }
}
