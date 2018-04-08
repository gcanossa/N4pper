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
        public DateTimeOffset MeetingDay { get; set; }
        public double Score { get; set; }
    }
}
