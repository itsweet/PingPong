using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyTest
{
    class MC100
    {
        [DllImport("mc100.dll", EntryPoint = "mc100_scan_device")]
        public static extern int mc100_scan_device();
        [DllImport("mc100.dll", EntryPoint = "mc100_open")]
        public static extern int mc100_open(int id);
        [DllImport("mc100.dll", EntryPoint = "mc100_close")]
        public static extern int mc100_close(int id);
        [DllImport("mc100.dll", EntryPoint = "mc100_set_pin")]
        public static extern int mc100_set_pin(int id, int pin);
        [DllImport("mc100.dll", EntryPoint = "mc100_clear_pin")]
        public static extern int mc100_clear_pin(int id, int pin);
        [DllImport("mc100.dll", EntryPoint = "mc100_check_pin")]
        public static extern int mc100_check_pin(int id, int pin);
        [DllImport("mc100.dll", EntryPoint = "mc100_set_push_pull")]
        public static extern int mc100_set_push_pull(int id, int port, int value);
        [DllImport("mc100.dll", EntryPoint = "mc100_set_pull_up")]
        public static extern int mc100_set_pull_up(int id, int port, int value);
        [DllImport("mc100.dll", EntryPoint = "mc100_read_port")]
        public static extern int mc100_read_port(int id, int port);
        [DllImport("mc100.dll", EntryPoint = "mc100_write_port")]
        public static extern int mc100_write_port(int id, int port, int value);

        const int MC100_PORTA = 0;
        const int MC100_PORTB = 1;
        const int MC100_PORTC = 2;

        public static void Remote_action()
        {
            int n = mc100_open(0);
            if (n < 0)
            {
                Console.WriteLine("打开设备失败!" + n);
                return;
            }
            RemoteCon(0);
            //Thread.Sleep(2000);
            RemoteCon(1);
            Thread.Sleep(5000);
            RemoteCon(0);
            Thread.Sleep(2000);
            RemoteCon(2);
            mc100_close(0);
        }
        public static void RemoteCon(int status)
        {
            switch (status)
            {
                case 0:
                    mc100_write_port(0, MC100_PORTA, 0xff);
                    break;
                case 1:
                    mc100_write_port(0, MC100_PORTA, ~0x40);
                    break;
                case 2:
                    mc100_write_port(0, MC100_PORTA, ~0x20);
                    break;
            }
            Console.WriteLine("read port value:" + mc100_read_port(0, MC100_PORTA));

        }
    }
    
}
