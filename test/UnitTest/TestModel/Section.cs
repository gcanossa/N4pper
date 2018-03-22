using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class Section : EditableEntityBase
    {
        public Chapter Chapter { get; set; }
        public List<IContent> Contents { get; set; }
    }
}
