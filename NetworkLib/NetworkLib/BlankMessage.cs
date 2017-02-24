using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLib
{
    public class BlankMessage : Message
    {
        public override void Parse()
        {
            throw new NotImplementedException();
        }
    }
}
