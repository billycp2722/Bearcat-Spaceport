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
using System.Diagnostics;

namespace Rocket_TM_BSC.Model
{
    public class Cap2_TM
    {
        private BackgroundWorker TMDataWorker;
        public Cap2_TM()
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
        public ConcurrentQueue<string> TM2Data;
        public ConcurrentQueue<string> CommandStringTM2;
        private string command_on = "ON";
        public Cap2_DataProcessing_Hex cap2_DataProcessing_Hex;
        public int lost_frames = 0;
        private void TMDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                
                CommandStringTM2 = new ConcurrentQueue<string>();
                cap2_DataProcessing_Hex = new Cap2_DataProcessing_Hex();
                //cap1_DataProcessing.Cap1DataProcessor.RunWorkerAsync();
                cap2_DataProcessing_Hex.StartCap1DataProcess();
                TM2Data = new ConcurrentQueue<string>();

                _serialport = new SerialPort(comport, 230400, Parity.None, 8, StopBits.One);

                _serialport.Open();
                _serialport.DiscardNull = true;
                _serialport.DiscardInBuffer();
                _serialport.ReadTimeout = 500;

                //_serialport.WriteLine("transmit_data");
                int FrameCount = 0;
                //Stopwatch stopwatch = new Stopwatch();
                
               
                while (_serialport.IsOpen)
                {
                    //int bytesToRead = 29; // TM Data length
                    if (CommandStringTM2.Count > 0)
                    {
                        CommandStringTM2.TryDequeue(out string command);
                        _serialport.WriteLine(command);
                    }

                    int bytesToRead = 68;
                    byte[] buffer = new byte[bytesToRead];
                    int bytesRead = 0;
                    while (bytesRead < bytesToRead) 
                    {
                        //Console.WriteLine(bytesRead);
                        try
                        {
                            bytesRead += _serialport.BaseStream.Read(buffer, bytesRead, bytesToRead - bytesRead);
                        }
                        catch
                        {

                        }
                        
                        //Console.WriteLine("Wait");
                        //Console.WriteLine(bytesRead);
                        if(CommandStringTM2.Count > 0) 
                        {
                            CommandStringTM2.TryDequeue(out string command);
                            _serialport.WriteLine(command);
                        }
                        

                    }
                    //stopwatch.Start();
                    //FrameCount++;
                    //if (stopwatch.ElapsedMilliseconds >= 2000)
                    //{
                    //    stopwatch.Stop();
                    //    //Console.WriteLine(FrameCount / 2);
                    //}

                    //Console.Write(Encoding.UTF8.GetString(buffer));
                    //byte[] CheckByte = new byte[1] { buffer[78] };
                    //if (Encoding.UTF8.GetString(CheckByte) == "\n")
                    //{
                    cap2_DataProcessing_Hex.Cap2DataQueue_Hex.Enqueue(buffer);
                    //}
                    //else
                    //{
                    //    _serialport.DiscardInBuffer();
                    //    lost_frames++;
                    //    Console.WriteLine("Lost Frame: " + lost_frames);
                    //}

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
