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
        public Cap1_DataProcessing_Hex cap1_DataProcessing_Hex;
        public int lost_frames = 0;
        private bool SerialFlag2 = false;
        private bool LostFrameFlag = false;
        public double FrameRate = 0;
        private Stopwatch sw_Cap1;  
        private int FrameCount = 0;
        
        private void TMDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (SerialFlag2 == false) 
                {
                    sw_Cap1 = new Stopwatch();
                    CommandStringTM1 = new ConcurrentQueue<string>();
                    cap1_DataProcessing_Hex = new Cap1_DataProcessing_Hex();
                    //cap1_DataProcessing.Cap1DataProcessor.RunWorkerAsync();
                    cap1_DataProcessing_Hex.StartCap1DataProcess();
                    TMData = new ConcurrentQueue<string>();

                    _serialport = new SerialPort(comport, 230400, Parity.None, 8, StopBits.One);

                    _serialport.Open();
                    _serialport.DiscardNull = true;
                    _serialport.DiscardInBuffer();
                    _serialport.ReadTimeout = 500;
                    SerialFlag2 = true;
                    sw_Cap1.Start();
                }
                

                //_serialport.WriteLine("transmit_data");
                
                //Stopwatch stopwatch = new Stopwatch();
                
                
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
                        try
                        {
                            bytesRead += _serialport.BaseStream.Read(buffer, bytesRead, bytesToRead - bytesRead);
                            if (LostFrameFlag)
                            {
                               if ((char)buffer[bytesRead-1] == '\n')
                               {
                                    LostFrameFlag = false;
                                    break;
                               }
                               
                            }
                        }
                        catch
                        {
                        }
                        
                        //Console.WriteLine("Wait");
                        //Console.WriteLine(bytesRead);
                        if(CommandStringTM1.Count > 0) 
                        {
                            CommandStringTM1.TryDequeue(out string command);
                            _serialport.WriteLine(command);
                        }
                        

                    }

                    // Make Sure this functions as Intended
                    if (buffer.Length == 78)
                    {
                        FrameCount++;
                        if ((char)buffer[77] == '\n')
                        {
                            cap1_DataProcessing_Hex.Cap1DataQueue_Hex.Enqueue(buffer);
                        }
                        else
                        {
                            LostFrameFlag = true;
                            lost_frames++;
                            Console.WriteLine("Lost Frame: " + lost_frames);
                        }
                    }
                    else
                    {
                        lost_frames++;
                        Console.WriteLine("Lost Frame: " + lost_frames);
                    }

                    if (sw_Cap1.ElapsedMilliseconds >= 2000)
                    {
                        FrameRate = FrameCount / (sw_Cap1.ElapsedMilliseconds / 1000);
                        FrameCount = 0;
                        sw_Cap1.Restart();
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
