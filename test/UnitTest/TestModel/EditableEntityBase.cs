using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public abstract class EditableEntityBase : IEditableEntity
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public User Owner { get; set; }
        public List<User> Contributors { get; set; }
    }
}
