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
    public class Rocket_TM
    {
        private BackgroundWorker TMDataWorker2;
        public Rocket_TM()
        {
            TMDataWorker2 = new BackgroundWorker();
            TMDataWorker2.DoWork += TMDataWorker_DoWork;
            TMDataWorker2.RunWorkerCompleted += TMDataWorker_RunWorkerCompleted; 
            TMDataWorker2.ProgressChanged += TMDataWorker_ProgressChanged;
            TMDataWorker2.WorkerSupportsCancellation = true;
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
        public ConcurrentQueue<string> TM3Data;
        public ConcurrentQueue<string> CommandStringTM3;
        private string command_on = "ON";
        public Rocket_DataProcessing_Hex Rocket_DataProcessing_Hex;
        public int lost_frames_rocket = 0;
        private bool flag = false;
        private SerialPort _serialport2;
        private void TMDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (flag == false)
                {
                    CommandStringTM3 = new ConcurrentQueue<string>();
                    Rocket_DataProcessing_Hex = new Rocket_DataProcessing_Hex();
                    //cap1_DataProcessing.Cap1DataProcessor.RunWorkerAsync();
                    Rocket_DataProcessing_Hex.StartCap1DataProcess();
                    TM3Data = new ConcurrentQueue<string>();

                    _serialport2 = new SerialPort(comport, 230400, Parity.None, 8, StopBits.One);

                    _serialport2.Open();
                    _serialport2.DiscardNull = true;
                    _serialport2.DiscardInBuffer();
                    _serialport2.ReadTimeout = 500;
                    flag = true;
                }
                

                //_serialport.WriteLine("transmit_data");
                int FrameCount = 0;
                //Stopwatch stopwatch = new Stopwatch();
                
               
                while (_serialport2.IsOpen)
                {
                    
                    //int bytesToRead = 29; // TM Data length
                    if (CommandStringTM3.Count > 0)
                    {
                        CommandStringTM3.TryDequeue(out string command);
                        _serialport2.WriteLine(command);
                    }

                    int bytesToRead = 66;
                    byte[] buffer = new byte[bytesToRead];
                    int bytesRead = 0;
                    while (bytesRead < bytesToRead) 
                    {
                        //Console.WriteLine(bytesRead);
                        try
                        {
                            bytesRead += _serialport2.BaseStream.Read(buffer, bytesRead, bytesToRead - bytesRead);
                        }
                        catch
                        {

                        }
                        
                        //Console.WriteLine("Wait");
                        //Console.WriteLine(bytesRead);
                        if(CommandStringTM3.Count > 0) 
                        {
                            CommandStringTM3.TryDequeue(out string command);
                            _serialport2.WriteLine(command);
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
                    if ((char)buffer[65] == '\n')
                    {
                        Rocket_DataProcessing_Hex.RocketDataQueue_Hex.Enqueue(buffer);
                    }
                    else
                    {
                        _serialport2.DiscardInBuffer();
                        lost_frames_rocket++;
                        Console.WriteLine("Lost Frame: " + lost_frames_rocket);
                    }
                    
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
                Console.WriteLine("Cap2:" +ex.ToString());

            }
        }

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
            TMDataWorker2.RunWorkerAsync();
        }
    }
}
