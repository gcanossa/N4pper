using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public interface IContent : IEditableEntity
    {
        string Text { get; set; }
    }
}
