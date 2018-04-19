using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    public class Friend
    {
        public User Who => Destination as User;
        public User Of => Source as User;
        public virtual DateTimeOffset MeetingDay { get; set; }
        public virtual double Score { get; set; }

        public virtual object Source { get; set; }
        public virtual object Destination { get; set; }
        public virtual long? EntityId { get; set; }

        public virtual string SourcePropertyName { get; set; }
        public virtual string DestinationPropertyName { get; set; }
        public virtual int Order { get; set; }
        public virtual long Version { get; set; }
    }
}
