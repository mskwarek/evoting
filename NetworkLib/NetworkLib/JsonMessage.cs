using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLib
{
    public class JsonMessage
    {
        public String header;
        public String data;

        public JsonMessage(String header, String data)
        {
            this.header = header;
            this.data = data;
        }
    }
}
