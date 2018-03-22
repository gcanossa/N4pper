using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public interface IEditableEntity
    {
        int Id { get; set; }
        int Index { get; set; }
        User Owner { get; set; }
        List<User> Contributors { get; set; }
    }
}
