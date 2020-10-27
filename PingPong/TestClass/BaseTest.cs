using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PingPong.TestClass
{
    class BaseTest
    {
        ZingoTIFUART tIFUART;

        public BaseTest(ZingoTIFUART tIFUART,string shortid)
        {
            this.tIFUART = tIFUART;
        }
        public void BindCMD(string source, string shortid, string destination, Action<string> action)
        {
            tIFUART.BindCMD(source, shortid, destination, action);

        }
        public void ReportCMD(string shortid)
        {
            tIFUART.ReportCMD(shortid);
        }

        public void GetMacCmd(string shortid)
        {
            tIFUART.GetMacCmd(shortid);
        }

        public string GetMac(string data)
        {
            return tIFUART.GetMac(data);

        }

        public void Leave(string shortid)
        {
            tIFUART.Leave(shortid);
        }


        public string GetShortID(string data)
        {
            return tIFUART.GetShortID(data);
        }

    }
}
