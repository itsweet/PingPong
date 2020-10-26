using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using PingPong.TestClass;
using System.Collections;

namespace PingPong
{

    
    public partial class Form1 : Form
    {
        MySerial mySerial ;
        static int time = 5000;
        string shortid="";
        string mypath;//log存放路径
        LinkedList<string> macs = new LinkedList<string>();
        Hashtable hashtable = new Hashtable(); //设备短地址和Mac地址对应表


        public Form1()
        {
            InitializeComponent();
           
        }

        
        private void button1_Click(object sender, EventArgs e)
        {

        }
        public void addText(string text)
        {
            if (richTextBox1 != null )
            {
                Invoke(new Action(() => richTextBox1.AppendText(text+"\n")));
            }
        }

        void addREDText(string text)
        {
            Invoke(new Action(() =>
            {
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.SelectionLength = 0;
                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.AppendText(text+ "\n");
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
            }));
        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {
            OpenSerialDialog();
        }

        void OpenSerialDialog()
        {
            Form_Serial form_Serial = new Form_Serial();
            if (form_Serial.ShowDialog() == DialogResult.OK)
            {
                mySerial = MySerial.GetMySerial(form_Serial.GetCom(), 115200);
                mySerial.RevData += (data) =>
                {
                    DateTime time = DateTime.Now;
                    string timedata;
                    timedata = time.ToString() + " " + data;
                    File.AppendAllText(mypath,timedata);
                    if (data.Contains("ERROR"))
                    {
                        addREDText(timedata);
                    }
                    else
                    {
                        addText(timedata);
                    }

                    BlindHandle(data);
                    //BulbHandle(data);
                };
                OpenNetwork();
                
            }
        }
        void NetworkLeave()
        {
            //mySerial.asyncSend("plugin network-creator form 0 0x1234 0 11");
            //mySerial.asyncSend("net pjoin 255");
            mySerial.asyncSend("network leave");
        }
        void NetworkCreate()
        {
            mySerial.asyncSend("network form 11 0 0x1234");
        }
        void OpenNetwork()
        {
            //mySerial.asyncSend("net pjoin 255");
            mySerial.asyncSend("plugin network-creator-security open-network");

        }

        string[] GetImformathion(string data)
        {
            if (data.Contains("Short ID"))
            {
                string[] temp = data.Split(',');
                if (temp[0].Contains("Short ID"))
                {
                    string[] idtemp = temp[0].Split(':');
                    shortid = idtemp[1].Trim();
                }
                if (temp[1].Contains("EUI64"))
                {
                    string[] mactemp = temp[1].Split(')');
                    string mac = mactemp[1];
                    if (!macs.Contains(mac))
                    {
                        macs.AddLast(mac);
                        return new string[] { shortid, mac };
                    }
                }
                return new string[] { shortid };
            }
            if (data.Contains("Device Announce"))
            {
                data = data.Trim();
                string[] s = data.Split(':');
                return new string[] { s[1] };
            }
            return new string[0];


        }

        void BulbHandle(string data)
        {
            
            if (data.Contains("Device Announce"))
            {
                string[] information = GetImformathion(data);
                if (information.Length <1)
                {
                    return;
                }
                string id = information[0];
                new BulbTest(mySerial, id);                
            }
        }

        void BlindHandle(string data)
        {
            BlindTest blind = new BlindTest(mySerial);
            blind.GetMacCmd("0x0000");
            hashtable.Add("", "0x0000");
            mySerial.AddCallback(new Action<string>((s) =>
            {
                if (!s.Contains("Device Announce"))
                {
                    string shortid = blind.GetShortID(s);
                    hashtable.Add("", shortid);
                    GetMacCmd(shortid);
                };
                if (s.Contains("IEEE Address response"))
                {
                    string mac = GetMac(s);
                    if (hashtable.ContainsKey(""))
                    {
                        hashtable.Add(mac, hashtable[""]);
                        hashtable.Remove("");
                    }
                }
            }));
            string[] id = GetImformathion(data);
            string sourceMac = "", destination = "";
            if (id.Length<1)
            {
                return;
            }
            shortid = id[0];
            Action<string> back, back1,back2;
            back2 = (report_s) => {
                if (report_s.Contains("command 0x8021") && report_s.Contains("status: 0x00"))
                {
                    blind.ReportCMD();
                    mySerial.ClearCallback();
                }
            };
            back1 = (des_s) => {
                if (!des_s.Contains("IEEE Address response"))
                {
                    return;
                }
                destination = blind.GetMac(des_s);
                //Thread.Sleep(10 * 1000);
                BaseTest.BindCMD(sourceMac, shortid,destination,back2);
            };
            back = (s) =>
            {
                if (!s.Contains("IEEE Address response"))
                {
                    return;
                }
                sourceMac = bulbTest.GetMac(s);

                bulbTest.GetMacCmd(back1);
                //mySerial.callBack.action = null;
            };
            if (id != null )
            {
                shortid = id[0];
                bulbTest.GetMacCmd("0x0000", back);
            }
            

            if (data.Contains("clus 0x0102"))
            {
                if (!data.Contains("payload"))
                {
                    return;
                }
                data = data.Replace("payload", "@");
                string[] temp = data.Split('@')[1].Split(' ','[',']');
                if (temp.Length !=4)
                {
                    return;
                }
                string positon = temp[3];

                //addREDText("positon: %" + positon );
            }
            
        }
        Task BlindTestCMD(string shortid)
        {
            return Task.Run(new Action(() =>
            {
                int i = time;
                while (i-- > 0)
                {
                    if (mySerial.cts.IsCancellationRequested)
                    {
                        return;
                    }
                    mySerial._resetEvent.WaitOne();
                    //mySerial.Send("zdo power " + shortid);
                    try
                    {
                        mySerial.asyncSend("zcl window-covering  go-to-lift-percent 50");
                        Thread.Sleep(500);
                        mySerial.asyncSend("send " + shortid + " 1 1");
                        Thread.Sleep(3000);
                        mySerial.asyncSend("zcl global read 0x0102 0x0008");
                        mySerial.asyncSend("send " + shortid + " 1 1");
                        Thread.Sleep(1000);

                        mySerial.asyncSend("zcl window-covering  go-to-lift-percent 52");
                        Thread.Sleep(500);
                        mySerial.asyncSend("send " + shortid + " 1 1");
                        Thread.Sleep(3000);
                        mySerial.asyncSend("zcl global read 0x0102 0x0008");
                        mySerial.asyncSend("send " + shortid + " 1 1");
                        Thread.Sleep(1000);

                    }
                    catch(Exception e)
                    {

                    }
                }
            }),mySerial.cts.Token);
            
        }


       
        private void Form1_Load(object sender, EventArgs e)
        {
            string time = string.Format("{0:yyyyMMdd-HHmm}", DateTime.Now);
            mypath = "Log\\" + time + ".txt";
            string path = Directory.GetCurrentDirectory();
            if (!Directory.Exists(path + "\\Log"))
            {
                Directory.CreateDirectory(path + "\\Log");
            }
            OpenSerialDialog();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void clearTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            mySerial.Pause();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mySerial.Restart();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BlindTestCMD(shortid);
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}
