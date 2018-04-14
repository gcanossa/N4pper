using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public abstract class EditableEntityBase : IEditableEntity
    {
        public virtual long? EntityId { get; set; }
        public virtual int Index { get; set; }
        public virtual User Owner { get; set; }
        public virtual ICollection<User> Contributors { get; set; }
    }
}
