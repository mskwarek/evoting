using Newtonsoft.Json;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLib
{
    public class MessageSLTokens : Message
    {
        public String SL;
        public List<BigInteger> tokens;
        public List<BigInteger> exponents;

        public MessageSLTokens(BigInteger SL, List<BigInteger> tokens, List<BigInteger> exponents)
        {
            this.SL = SL.ToString();
            this.tokens = tokens;
            this.exponents = exponents;
        }

        public override void Parse()
        {
            throw new NotImplementedException();
        }
    }
}
