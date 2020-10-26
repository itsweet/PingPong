using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace PingPong
{
    public partial class Form_Serial : Form
    {
        public Form_Serial()
        {
            InitializeComponent();
        }

        private void Form_Serial_Load(object sender, EventArgs e)
        {
            refreshport();
        }

        public void refreshport()
        {
            List<string> portlist = new List<string> { };
            foreach (string portname in SerialPort.GetPortNames())
            {
                portlist.Add(portname);
            }
            
            comboBox1.DataSource = portlist;
            
        }

        public string GetCom()
        {
            return comboBox1.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
