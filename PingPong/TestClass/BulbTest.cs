using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PingPong.TestClass
{
    enum BublCMD
    {
        MoveToLevel,
        MoveToColorTemp,
        Leave,
        Remove
    }
    class BulbTest : BaseTest
    {
        int status = 0;
        string shortid;
        ZingoTIFUART zingoTIFUART;
        Action<string> BulbTestAction;
        public BulbTest(ZingoTIFUART mySerial, string shortid) : base(mySerial,shortid)
        {
            zingoTIFUART = mySerial;
            this.shortid = shortid;

            //BulbFFTest();
            //BulbOnOffTest(2500);
            BulbTestAction = new Action<string>((s) =>
            {
                if (!IsResponse(s))
                {
                    return;
                }
                if (s.Contains("command 0x8034") && s.Contains("status: 0x00"))
                {
                    if ((BublCMD)status == BublCMD.Leave)
                    {
                        status++;
                    }
                }
                else if (s.Contains("Level Control") && s.Contains("cmd 0B payload[00 00 ]"))
                {
                    if ((BublCMD)status == BublCMD.MoveToLevel)
                    {
                        status++;
                    }
                }
                else if (s.Contains("Color Control") && s.Contains("cmd 0B payload[0A 00 ]"))
                {
                    if ((BublCMD)status == BublCMD.MoveToColorTemp)
                    {
                        status++;
                    }
                }
                else
                {
                    return;
                }
                switch ((BublCMD)status)
                {
                    case BublCMD.MoveToLevel:
                        MoveToLevel();
                        break;
                    case BublCMD.MoveToColorTemp:
                        MoveToColorTemp();
                        break;
                    case BublCMD.Leave:
                        Leave(shortid);
                        break;
                    case BublCMD.Remove:
                        zingoTIFUART.RemoveCallback(BulbTestAction);
                        break;
                    default:
                        break;
                }
            });
            //zingoTIFUART.AddCallback(BulbTestAction);

            BulbTestCMD();

        }

        Task BulbTestCMD()
        {
            return Task.Run(new Action(() =>
            {
                Thread.Sleep(1 * 1000);
                MoveToLevel();
                Thread.Sleep(500);
                MoveToColorTemp();
                
            }));
        }

        Task BulbOnOffTest(int num)
        {
            return Task.Run(new Action(
                () =>
                {
                    while(num-- > 0)
                    {
                        zingoTIFUART.asyncSend("zcl on-off on");
                        zingoTIFUART.asyncSend("send " + shortid + " 1 1");
                        Thread.Sleep(7 * 1000);
                        zingoTIFUART.asyncSend("zcl on-off off");
                        zingoTIFUART.asyncSend("send " + shortid + " 1 1");
                        Thread.Sleep(7 * 1000);
                    }
                }
                ));
        }

        Task BulbFFTest()
        {
            return Task.Run(new Action(() =>
            {
                while (true)
                {
                    Thread.Sleep(3000);
                    zingoTIFUART.asyncSend("zcl level-control mv-to-level 0 0");
                    zingoTIFUART.asyncSend("send 0xffff 1 1");
                    Thread.Sleep(3000);
                    zingoTIFUART.asyncSend("zcl color-control movetocolortemp 200 0");
                    zingoTIFUART.asyncSend("send 0xffff 1 1");
                }
            }));
        }

        void MoveToLevel()
        {
            zingoTIFUART.asyncSend("zcl level-control mv-to-level 0 0");
            //Thread.Sleep(500);
            zingoTIFUART.asyncSend("send " + shortid + " 1 1");
        }
        void MoveToColorTemp()
        {
            zingoTIFUART.asyncSend("zcl color-control movetocolortemp 200 0");
            //Thread.Sleep(500);
            zingoTIFUART.asyncSend("send " + shortid + " 1 1");
        }
        bool IsResponse(string s)
        {
            return (s.Contains("RX") && s.Contains(shortid));
            
        }


    }
}
