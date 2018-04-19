using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public interface IEditableEntity
    {
        int Index { get; set; }
        User Owner { get; set; }
        ICollection<User> Contributors { get; set; }
    }
}
