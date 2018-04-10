using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class Explaination : EditableEntityBase, IContent
    {
        public virtual string Text { get; set; }
        public virtual double Relevance { get; set; }
    }
}
