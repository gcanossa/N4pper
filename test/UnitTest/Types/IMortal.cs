using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Types
{
    public interface IMortal
    {
        DateTime Birthday { get; }
        DateTime? Deathday { get; }
    }
}
