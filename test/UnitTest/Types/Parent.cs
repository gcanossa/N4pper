using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Types
{
    public class Parent : Person
    {
        List<Child> Children { get; set; }
    }
}
