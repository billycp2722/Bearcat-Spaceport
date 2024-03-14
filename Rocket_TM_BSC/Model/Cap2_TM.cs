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
        private BackgroundWorker TMDataWorker2;
        public Cap2_TM()
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
        public ConcurrentQueue<string> TM2Data;
        public ConcurrentQueue<string> CommandStringTM2;
        private string command_on = "ON";
        public Cap2_DataProcessing_Hex cap2_DataProcessing_Hex;
        public int lost_frames2 = 0;
        private bool flag = false;
        private SerialPort _serialport2;
        private int FrameCount = 0;
        public double FrameRate2 = 0;
        private bool LostFrameFlag = false;
        private Stopwatch sw_Cap2;
        private void TMDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (flag == false)
                {
                    CommandStringTM2 = new ConcurrentQueue<string>();
                    cap2_DataProcessing_Hex = new Cap2_DataProcessing_Hex();
                    //cap1_DataProcessing.Cap1DataProcessor.RunWorkerAsync();
                    cap2_DataProcessing_Hex.StartCap1DataProcess();
                    TM2Data = new ConcurrentQueue<string>();

                    _serialport2 = new SerialPort(comport, 230400, Parity.None, 8, StopBits.One);

                    _serialport2.Open();
                    _serialport2.DiscardNull = true;
                    _serialport2.DiscardInBuffer();
                    _serialport2.ReadTimeout = 500;
                    flag = true;
                    sw_Cap2.Start();
                }
                

                //_serialport.WriteLine("transmit_data");
                int FrameCount = 0;
                //Stopwatch stopwatch = new Stopwatch();
                
               
                while (_serialport2.IsOpen)
                {
                    
                    //int bytesToRead = 29; // TM Data length
                    if (CommandStringTM2.Count > 0)
                    {
                        CommandStringTM2.TryDequeue(out string command);
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
                            if (LostFrameFlag)
                            {
                                if ((char)buffer[bytesRead - 1] == '\n')
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
                        if(CommandStringTM2.Count > 0) 
                        {
                            CommandStringTM2.TryDequeue(out string command);
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
                    if (buffer.Length = bytesToRead)
                    {
                        FrameCount++;
                        if ((char)buffer[65] == '\n')
                        {
                            cap2_DataProcessing_Hex.Cap2DataQueue_Hex.Enqueue(buffer);
                        }
                        else
                        {
                            LostFrameFlag = true;
                            lost_frames2++;
                            Console.WriteLine("Lost Frame: " + lost_frames2);
                        }
                        
                    }
                    else
                    {
                        lost_frames2++;
                        Console.WriteLine("Lost Frame: " + lost_frames2);
                    }

                    if (sw_Cap2.ElapsedMilliseconds >= 2000)
                    {
                        FrameRate2 = FrameCount / (sw_Cap2.ElapsedMilliseconds / 1000);
                        FrameCount = 0;
                        sw_Cap2.Restart();
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
