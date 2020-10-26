using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        int status=0;
        MySerial mySerial;
        string shortid;
        Action<string> myAction = new Action<string>((s) =>
        {

        });
        public BlindTest(MySerial serial,string id) : base(serial)
        {
            mySerial = serial;
            shortid = id;
            serial.AddCallback(myAction);
        }

         

    }
}
