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
        ZingoTIFUART zingoTIFUART ;
        string mypath;//log存放路径
        LinkedList<DevicesInfo> deviceList = new LinkedList<DevicesInfo>();//设备短地址和Mac地址对应表

        struct DevicesInfo //设备短地址、Mac地址、是否绑定
        {
            public string shortid;
            public string mac;
            public bool test;
        }
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
                zingoTIFUART = new ZingoTIFUART(form_Serial.GetCom(), 115200);
                DevicesInfo devicesInfo = new DevicesInfo();
                devicesInfo.shortid = "0x0000";
                deviceList.AddLast(devicesInfo);
                //zingoTIFUART.GetMacCmd("0x0000");
                zingoTIFUART.AddCallback((data) =>
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
                    //JoinHandle(data);
                    //BlindHandle(data);
                    BulbHandle(data);
                });
                //zingoTIFUART.OpenNetwork();
            }
        }
        void BulbHandle(string data)
        {
            if (data.Contains("Device Announce"))
            {
                string shortid = zingoTIFUART.GetShortID(data);
                new BulbTest(zingoTIFUART, shortid);

            };
            
        }
        void JoinHandle(string data)
        {
            if (data.Contains("Device Announce"))
            {
                string shortid = zingoTIFUART.GetShortID(data);
                DevicesInfo devices = new DevicesInfo();
                devices.shortid = shortid;
                deviceList.AddLast(devices);
                zingoTIFUART.GetMacCmd(shortid);
                return;
            };
            //收到是获取MAC地址的回复
            if (data.Contains("IEEE Address response"))
            {
                string macadress = zingoTIFUART.GetMac(data);
                DevicesInfo info = deviceList.Last.Value;
                if (info.mac == null || info.mac == "")
                {
                    info.mac = macadress;
                }
                if (deviceList.Count > 1)
                {
                    string source = deviceList.First.Value.mac;
                    string dedestination = deviceList.Last.Value.mac;
                    string shortid = deviceList.Last.Value.shortid;
                    zingoTIFUART.BindCMD(source, shortid, dedestination);
                }
                return;
            };
            //收到绑定的回复
            if (data.Contains("command 0x8021") && data.Contains("status: 0x00"))
            {
                string shortid = deviceList.Last.Value.shortid;
                zingoTIFUART.ReportCMD(shortid);
                return;
                
            }
            //收到send-me-a-report的回复
            if (data.Contains("cmd 07") && data.Contains("payload[00 ]") && deviceList.Count>1)
            {
                DevicesInfo info = deviceList.Last.Value;
                if (info.test == false)
                {
                    info.test = true;
                    deviceList.RemoveLast();
                    deviceList.AddLast(info);
                    new BlindTest(zingoTIFUART, info.shortid, 3000*1000);
                }
            }
        }
        void BlindHandle(string data)
        {
            
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

            zingoTIFUART.Pause();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            zingoTIFUART.Restart();
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}
