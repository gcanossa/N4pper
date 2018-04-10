using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class Exercise :  EditableEntityBase, IContent
    {
        public virtual string Text { get; set; }
        public virtual double Value { get; set; }
    }
}
