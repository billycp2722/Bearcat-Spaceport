using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace Rocket_TM_BSC.Model
{
    public class SerialPort_TMData
    {
        private BackgroundWorker TMDataWorker;
        public SerialPort_TMData()
        {
            TMDataWorker = new BackgroundWorker();
            TMDataWorker.DoWork += TMDataWorker_DoWork;
            TMDataWorker.RunWorkerCompleted += TMDataWorker_RunWorkerCompleted; 
            TMDataWorker.ProgressChanged += TMDataWorker_ProgressChanged;
            TMDataWorker.WorkerSupportsCancellation = true;
        }

        private void TMDataWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }
        public string Data1;
        public string Data2;
        public string Data3;
        public string Data4;
        public string Data5;
        public bool WakeCommand = false;
        private void TMDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }
        private int flag = 0;
        private string comport;
        public ConcurrentQueue<string> TMData = new ConcurrentQueue<string>();
        private void TMDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (flag == 0)
                {
                    _serialport = new SerialPort(comport, 57600, Parity.None, 8, StopBits.One);
                    _serialport.Open();
                    flag = 1;
                }
                while (_serialport.IsOpen)
                {
                    int bytesToRead = 29; // TM Data length
                    byte[] buffer = new byte[bytesToRead];
                    _serialport.BaseStream.Read(buffer, 0, bytesToRead);

                    string receivedData = Encoding.UTF8.GetString(buffer);
                    if (receivedData.Length == 0 || receivedData == "" || receivedData == null) 
                    { }
                    else
                    {
                        string[] tmp_string = receivedData.Split('\n'); // n number 
                        for (int i = 0; i < tmp_string.Length; i++)
                        {
                            // Queue data for processing
                            TMData.Enqueue(tmp_string[i]);
                        }
                        // Add each line to queue for processing?

                    }
                    Console.WriteLine(receivedData);

                    //string receivedValues = _serialport.ReadLine();
                    //string[] values = receivedValues.Split(',');

                    
                }
                
            }
            catch
            {

            }
        }

        private SerialPort _serialport;
        
        public void OpenNewPort(string COM, int Baud, Parity parity, int dataBits, StopBits stopBits)
        {
            try
            {
                
                comport = COM;

            }
            catch (Exception ex)
            {
                return;
            }

            TMDataWorker.RunWorkerAsync();
        }
    }
}
