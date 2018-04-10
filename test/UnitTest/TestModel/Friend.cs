using System;
using System.Collections.Generic;
using System.Text;
using N4pper.Ogm.Entities;

namespace UnitTest.TestModel
{
    public class Friend : ExplicitConnection<User, User>
    {
        public User Who => Destination;
        public User Of => Source;
        public virtual DateTimeOffset MeetingDay { get; set; }
        public virtual double Score { get; set; }
    }
}
