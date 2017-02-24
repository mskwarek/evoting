using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLib
{
    public abstract class Message : IMessage
    {
        string IMessage.Serialize<T>(T deserialized)
        {
            return JsonConvert.SerializeObject(deserialized);
        }

        T IMessage.Deserialize<T>(string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public abstract void Parse();
    }
}
