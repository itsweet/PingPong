using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PingPong.TestClass
{
    enum BlindCMD
    {
        
        ReportCMD,
        BindCMD,
        BindTestCMD
    }
    class BlindTest : BaseTest
    {
        ZingoTIFUART zingoTIFUART;
        string shortid;
        int time;//测试次数
        public BlindTest(ZingoTIFUART serial, string id,int time) : base(serial, id)
        {
            this.time = time;
            zingoTIFUART = serial;
            shortid = id;
            BlindTestCMD(shortid);
        }

        Task BlindTestCMD(string shortid)
        {
            return Task.Run(new Action(() =>
            {
                int i = time;
                while (i-- > 0)
                {
                    if (zingoTIFUART.cts.IsCancellationRequested)
                    {
                        return;
                    }
                    zingoTIFUART._resetEvent.WaitOne();
                    //mySerial.Send("zdo power " + shortid);
                    try
                    {
                        zingoTIFUART.asyncSend("zcl window-covering  go-to-lift-percent 50");
                        Thread.Sleep(500);
                        zingoTIFUART.asyncSend("send " + shortid + " 1 1");
                        Thread.Sleep(3000);
                        //zingoTIFUART.asyncSend("zcl global read 0x0102 0x0008");
                        //zingoTIFUART.asyncSend("send " + shortid + " 1 1");
                        //Thread.Sleep(1000);

                        zingoTIFUART.asyncSend("zcl window-covering  go-to-lift-percent 52");
                        Thread.Sleep(500);
                        zingoTIFUART.asyncSend("send " + shortid + " 1 1");
                        Thread.Sleep(3000);
                        //zingoTIFUART.asyncSend("zcl global read 0x0102 0x0008");
                        //zingoTIFUART.asyncSend("send " + shortid + " 1 1");
                        //Thread.Sleep(1000);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }), zingoTIFUART.cts.Token);

        }

    }
}
