using System;
using System.Collections.Generic;
using System.Text;

namespace N4pper
{
    public interface IRecordHandler
    {
        object ParseRecordValue(object value, Type assigningType, Type realType);
    }
}
