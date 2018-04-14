using N4pper.Ogm.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public interface IEditableEntity : IOgmEntity
    {
        int Index { get; set; }
        User Owner { get; set; }
        ICollection<User> Contributors { get; set; }
    }
}
