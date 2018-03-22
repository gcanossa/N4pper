using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class Explaination : EditableEntityBase, IContent
    {
        public string Text { get; set; }
        public double Relevance { get; set; }
    }
}
