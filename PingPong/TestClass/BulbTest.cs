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
        Leave
    }
    class BulbTest : BaseTest
    {
        int status = 0;
        string shortid;
        public BulbTest(MySerial mySerial, string shortid) : base(mySerial)
        {
            this.shortid = shortid;
            mySerial.AddCallback(new Action<string>((s)=>
            {
                if (!IsResponse(s))
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
                }
            }));
            MoveToLevel();
            status ++;
            //BulbFFTest();
            //BulbOnOffTest(2500);
        }
        Task BulbTestCMD(string shortid)
        {
            return Task.Run(new Action(() =>
            {
                Action<string> callBack,callBack1;
                callBack1 = (mvtolive_s) =>
                {
                    string[] temp = Regex.Split(mvtolive_s, "payload");
                    temp = temp[1].Replace("]","").Replace("[","").Split(' ');
                    if (temp[1] == "00")
                    {
                        mySerial.asyncSend("zcl level-control mv-to-level 0 0");
                        mySerial.asyncSend("send " + shortid + " 1 1");
                    }
                };
                mySerial.AddCallback ( callBack1);
                callBack = (movetocolor_s) =>
                {
                    mySerial.RemoveCallback ( callBack1);
                    mySerial.asyncSend("zcl color-control movetocolortemp 200 0");
                    mySerial.asyncSend("send " + shortid + " 1 1");
                    
                    mySerial.asyncSend("zdo leave " + shortid + " 1 0");
                };
                mySerial.AddCallback( callBack);
            }));
        }

        Task BulbOnOffTest(int num)
        {
            return Task.Run(new Action(
                () =>
                {
                    while(num-- > 0)
                    {
                        mySerial.asyncSend("zcl on-off on");
                        mySerial.asyncSend("send " + shortid + " 1 1");
                        Thread.Sleep(5000);
                        mySerial.asyncSend("zcl on-off off");
                        mySerial.asyncSend("send " + shortid + " 1 1");
                        Thread.Sleep(5000);
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
                    mySerial.asyncSend("zcl level-control mv-to-level 0 0");
                    mySerial.asyncSend("send 0xffff 1 1");
                    Thread.Sleep(3000);
                    mySerial.asyncSend("zcl color-control movetocolortemp 200 0");
                    mySerial.asyncSend("send 0xffff 1 1");
                }
            }));
        }

        void MoveToLevel()
        {
            mySerial.asyncSend("zcl level-control mv-to-level 0 0");
            Thread.Sleep(500);
            mySerial.asyncSend("send " + shortid + " 1 1");
        }
        void MoveToColorTemp()
        {
            mySerial.asyncSend("zcl color-control movetocolortemp 200 0");
            Thread.Sleep(500);
            mySerial.asyncSend("send " + shortid + " 1 1");
        }
        bool IsResponse(string s)
        {
            return (s.Contains("RX") && s.Contains(shortid));
            
        }


    }
}
