using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLib
{
    public interface IMessage
    {
        string Serialize<T>(T deserialized);
        T Deserialize<T>(string serialized);
    }
}
