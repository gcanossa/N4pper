using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.TestModel
{
    interface ITestEntity
    {
        int Id { get; set; }
        int Integer { get; set; }
        int? IntegerNullable { get; set; }
        double Double { get; set; }
        DateTime DateTime { get; set; }
        DateTime? DateTimeNullable { get; set; }
        DateTimeOffset DateTimeOffset { get; set; }
        DateTimeOffset? DateTimeOffsetNullable { get; set; }
        TimeSpan TimeSpan { get; set; }
        TimeSpan? TimeSpanNullable { get; set; }
        string String { get; set; }

        object Object { get; set; }
        int ReadonlyInt { get; }
        int WriteonlyInt { set; }
        TestEnum EnumValue { get; set; }

        ICollection<string> Values { get; set; }
    }
}
