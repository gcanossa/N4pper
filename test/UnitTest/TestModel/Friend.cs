using System;
using System.Collections.Generic;
using System.Text;
using N4pper.Ogm.Entities;

namespace UnitTest.TestModel
{
    public class Friend : IOgmConnection<User, User>
    {
        public User Who => Destination as User;
        public User Of => Source as User;
        public virtual DateTimeOffset MeetingDay { get; set; }
        public virtual double Score { get; set; }

        public virtual IOgmEntity Source { get; set; }
        public virtual IOgmEntity Destination { get; set; }
        public virtual long? EntityId { get; set; }

        public virtual string SourcePropertyName { get; set; }
        public virtual string DestinationPropertyName { get; set; }
        public virtual int Order { get; set; }
        public virtual long Version { get; set; }
    }
}
