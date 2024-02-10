using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.ComponentModel;
using System.Collections.Concurrent;
using SciChart.Core.Extensions;

namespace Rocket_TM_BSC.Model
{
    public class Cap1_TM
    {
        private BackgroundWorker TMDataWorker;
        public Cap1_TM()
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
        private int coun = 0;
        private void TMDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }
        private int flag = 0;
        private string comport;
        public ConcurrentQueue<string> TMData = new ConcurrentQueue<string>();
        private string command_on = "ON";
        private void TMDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (flag == 0)
                {
                    _serialport = new SerialPort(comport, 230400, Parity.None, 8, StopBits.One);
                    
                    _serialport.Open();
                    _serialport.DiscardNull = true; 
                    flag = 1;
                    _serialport.WriteLine("ON");
                }
                int counter = 0;
                while (_serialport.IsOpen)
                {
                    //int bytesToRead = 29; // TM Data length
                    int bytesToRead = 29;
                    byte[] buffer = new byte[bytesToRead];
                    int bytesRead = 0;
                    while (bytesRead < bytesToRead) 
                    {
                        bytesRead += _serialport.BaseStream.Read(buffer, bytesRead, bytesToRead-bytesRead);
                        
                    }
    
                    string receivedData = Encoding.UTF8.GetString(buffer);
                    //Console.WriteLine(buffer.Count<byte>());

                    counter++;
                    Console.WriteLine(counter + ": " + receivedData);
                    TMData.Enqueue(receivedData);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

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
