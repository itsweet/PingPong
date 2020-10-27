using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PingPong
{
    class ZingoTIFUART : MySerial
    {
        public ZingoTIFUART(string com, int rate) : base(com, rate)
        {
            
        }

        public void BindCMD(string source, string shortid, string destination, Action<string> action)
        {
            string bindcmd = string.Format("zdo bind {0} 1 1 0x0102 {{{1}}} {{{2}}}", shortid, destination, source);
            mySerial.asyncSend(bindcmd, action);

        }
        public void ReportCMD(string shortid)
        {
            string reportcmd = "zcl global send-me-a-report 0x0102 8 0x20 1 0 {00}";
            mySerial.asyncSend(reportcmd);
            Thread.Sleep(500);
            mySerial.asyncSend("send " + shortid + " 1 1");
        }

        public void GetMacCmd(string shortid)
        {
            mySerial.asyncSend("zdo ieee " + shortid);
        }

        public string GetMac(string data)
        {
            string mac = "";
            if (data.Contains("IEEE Address response"))
            {
                data = data.Trim('\r');
                data = data.Replace("(>)", "@");
                string[] temp = data.Split('@');
                mac = temp[1];
            }
            return mac;

        }

        public void Leave(string shortid)
        {
            mySerial.asyncSend("zdo leave " + shortid + " 1 0");
        }


        public string GetShortID(string data)
        {
            if (data.Contains("Device Announce"))
            {
                data = data.Trim();
                string[] s = data.Split(':');
                return s[1];
            }
            else
            {
                return "";
            }
        }

        public void NetworkCreate()
        {
            mySerial.asyncSend("network form 11 0 0x1234");
        }
        public void OpenNetwork()
        {
            //mySerial.asyncSend("net pjoin 255");
            mySerial.asyncSend("plugin network-creator-security open-network");

        }
    }
}
