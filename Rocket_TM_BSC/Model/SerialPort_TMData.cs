﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.ComponentModel;

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
                    //int bytesToRead = _serialport.BytesToRead;
                    //byte[] buffer = new byte[bytesToRead];
                    //Read(buffer,0,bytesToRead);
                    //_serialport.BaseStream.Read(buffer, 0, bytesToRead);

                    //string receivedData = Encoding.UTF8.GetString(buffer);
                    //Console.WriteLine(receivedData);
                        string receivedValues = _serialport.ReadLine();
                        string[] values = receivedValues.Split(',');

                        if (values.Length == 5)
                        {
                            Data1 = values[0];
                            Data2 = values[1];
                            Data3 = values[2];
                            Data4 = values[3];
                            Data5 = values[4];

                        }
                        else if (values.Length == 0)
                        {

                        }
                        else
                        {
                            Console.WriteLine("Bad Data Recieved");
                        }
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
