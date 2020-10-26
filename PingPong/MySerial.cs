using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PingPong
{

    class MySerial
    {
        static MySerial mySerial;
        SerialPort myPort;
        public CancellationTokenSource cts;
        public ManualResetEvent _resetEvent = new ManualResetEvent(true);
        public Action<string> RevData;
        private Action<string> callBack;


        public MySerial(string com,int rate)
        {
            cts = GetCancellationToken();
            if (myPort != null)
            {
                myPort.Close();
            }
            
            myPort = new SerialPort(com, rate);

            if (!myPort.IsOpen)
            {
                try
                {
                    myPort.Open();
                }
                catch(Exception e)
                {
                    MessageBox.Show("串口错误，请检查"+e, "警告");
                }
            }
            else
            {
                myPort.Close();
            }
            
            Task revTask = new Task(() =>
            {
                while (true)
                {
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }

                    // 初始化为true时执行WaitOne不阻塞
                    _resetEvent.WaitOne();

                    try
                    {
                        string data = myPort.ReadLine();
                        //rev.DataHandle(data);
                        RevData(data);
                        if (callBack == null)
                        {
                            continue;
                        }
                        callBack(data);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        Stop();
                    }
                    
                    
                }
            },cts.Token);
            revTask.Start();
        }

        public static MySerial GetMySerial(string com, int rate)
        {
            if (mySerial != null)
            {
                mySerial.Stop();
            }
            mySerial = new MySerial(com, rate);
            return mySerial;
        }

        CancellationTokenSource GetCancellationToken()
        {
            if (cts == null)
            {
                cts = new CancellationTokenSource();
            }
            return cts;
        }

        public void Send(string data) 
        {
            try
            {
                myPort.WriteLine(data);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                Stop();
            }
        }

        public async void asyncSend(string data)
        {
            await Task.Delay(1);
            Send(data);
        }
        public void asyncSend(string data, Action<string> call)
        {
            callBack += call;
            asyncSend(data);
        }

        public void Stop()
        {
            cts.Cancel();
        }

        public void Pause()
        {
            _resetEvent.Reset();
        }

        public void Restart()
        {
            _resetEvent.Set();
        }

        public void AddCallback(Action<string> action)
        {
            callBack += action;
        }

        public void RemoveCallback(Action<string> action)
        {
            callBack -= action;
        }

        public void ClearCallback()
        {
            callBack = null;
        }

    }
}
