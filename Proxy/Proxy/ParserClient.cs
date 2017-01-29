﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proxy
{
    class ParserClient
    {
        private Proxy proxy;

        public ParserClient(Proxy proxy)
        {
            this.proxy = proxy;
        }

        public void parseMessageFromClient(string msg)
        {
            string[] elem = msg.Split('&');
            switch (elem[0])
            {
                case NetworkLib.Constants.GET_SL_AND_SR:
                    this.proxy.sendSLAndSR(elem[1]);
                    break;
                case NetworkLib.Constants.VOTE:
                    this.proxy.saveVote(elem[1]);
                    break;

            }
        }
    }
}
