using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.ComponentModel;
using System.Collections.Concurrent;
using SciChart.Core.Extensions;
using System.Drawing.Printing;
using System.Collections.ObjectModel;

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
            // Not Used
        }
        public bool WakeCommand = false;
        
        private void TMDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Not Used
        }
        
        private string comport;
        public ConcurrentQueue<string> TMData;
        public ConcurrentQueue<string> CommandStringTM1;
        private string command_on = "ON";
        public Cap1_DataProcessing cap1_DataProcessing;
        public int lost_frames = 0;
        private void TMDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                
                CommandStringTM1 = new ConcurrentQueue<string>();
                cap1_DataProcessing = new Cap1_DataProcessing();
                //cap1_DataProcessing.Cap1DataProcessor.RunWorkerAsync();
                cap1_DataProcessing.StartCap1DataProcess();
                TMData = new ConcurrentQueue<string>();

                _serialport = new SerialPort(comport, 230400, Parity.None, 8, StopBits.One);

                _serialport.Open();
                _serialport.DiscardNull = true;
                _serialport.DiscardInBuffer();
                _serialport.WriteLine("start");


                
                while (_serialport.IsOpen)
                {
                    //int bytesToRead = 29; // TM Data length
                    if (CommandStringTM1.Count > 0)
                    {
                        CommandStringTM1.TryDequeue(out string command);
                        _serialport.WriteLine(command);
                    }

                    int bytesToRead = 78;
                    byte[] buffer = new byte[bytesToRead];
                    int bytesRead = 0;
                    while (bytesRead < bytesToRead) 
                    {
                        //Console.WriteLine(bytesRead);
                        bytesRead += _serialport.BaseStream.Read(buffer, bytesRead, bytesToRead-bytesRead);
                        //Console.WriteLine(bytesRead);
                        if(CommandStringTM1.Count > 0) 
                        {
                            CommandStringTM1.TryDequeue(out string command);
                            _serialport.WriteLine(command);
                        }
                        
                    }
                    byte[] CheckByte = new byte[2] { buffer[77], buffer[78] };
                    if (Encoding.UTF8.GetString(CheckByte) == "\n")
                    {
                        cap1_DataProcessing.Cap1DataQueue.Enqueue(buffer);
                    }
                    else
                    {
                        _serialport.DiscardInBuffer();
                        lost_frames++;
                    }

                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
        }

        private SerialPort _serialport;

        public void OpenNewPort(string COM)
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
