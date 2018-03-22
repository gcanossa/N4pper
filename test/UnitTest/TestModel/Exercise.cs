using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class Exercise :  EditableEntityBase, IContent
    {
        public string Text { get; set; }
        public double Value { get; set; }
    }
}
