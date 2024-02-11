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
        public string Data1;
        public string Data2;
        public string Data3;
        public string Data4;
        public string Data5;
        public bool WakeCommand = false;
        private int coun = 0;
        private void TMDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Not Used
        }
        private int flag = 0;
        private string comport;
        public ConcurrentQueue<string> TMData = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> CommandStringTM1 = new ConcurrentQueue<string>();
        private string command_on = "ON";

        private void TMDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (flag == 0)
                {
                    _serialport = new SerialPort(comport, 115200, Parity.None, 8, StopBits.One);
                    
                    _serialport.Open();
                    _serialport.DiscardNull = true;
                    _serialport.DiscardInBuffer();
                    flag = 1;
                    _serialport.WriteLine("ON");
                }
                int counter = 0;
                while (_serialport.IsOpen)
                {
                    //int bytesToRead = 29; // TM Data length
                    if (CommandStringTM1.Count > 0)
                    {
                        CommandStringTM1.TryDequeue(out string command);
                        _serialport.WriteLine(command);
                    }

                    int bytesToRead = 29;
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

                    // How slow is this?
                    //Console.WriteLine(buffer.Length);
                    byte[] Lat = new byte[4] { buffer[0], buffer[1], buffer[2], buffer[3] };
                    byte[] Lon = new byte[4] { buffer[4], buffer[5], buffer[6], buffer[7] };
                    byte[] MS = new byte[4] { buffer[8], buffer[9], buffer[10], buffer[11] };
                    byte SatCoun = buffer[12];
                    byte GyroX = buffer[13];
                    byte GyroY = buffer[14];
                    byte GyroZ = buffer[15];
                    byte[] AccelX = new byte[2] { buffer[16], buffer[17]};
                    byte[] AccelY = new byte[2] { buffer[18], buffer[19]};
                    byte[] AccelZ = new byte[2] { buffer[20], buffer[21]};
                    byte[] Alt = new byte[2] { buffer[22], buffer[23] };
                    byte[] VOC = new byte[2] { buffer[24], buffer[25]};
                    byte[] Temp = new byte[2] { buffer[26], buffer[27]};
                    byte Humid = buffer[28];


                    
                    int Lat_int = BitConverter.ToInt32 (Lat, 0);
                    int Lon_int = BitConverter.ToInt32(Lon, 0);
                    uint MS_int = BitConverter.ToUInt32(MS,0);
                    double SatCoun_int = SatCoun.ToDouble ();
                    double GyroX_int =GyroX.ToDouble ();
                    double GyroY_int = GyroY.ToDouble();
                    double GyroZ_int = GyroZ.ToDouble();
                    int AccelX_int = BitConverter.ToInt16(AccelX, 0);
                    int AccelY_int = BitConverter.ToInt16(AccelY, 0);
                    int AccelZ_int = BitConverter.ToInt16(AccelZ, 0);
                    uint Alt_int = BitConverter.ToUInt16(Alt, 0);
                    uint VOC_int = BitConverter.ToUInt16(VOC, 0);
                    int Temp_int = BitConverter.ToInt16(Temp, 0);
                    double Humid_int = Humid.ToDouble();

                    Console.WriteLine(Lat_int + "," + Lon_int + "," + MS_int + "," + SatCoun_int + "," + GyroX_int + "," + GyroY_int + "," + GyroZ_int + "," + AccelX_int + "," + AccelY_int + "," + AccelZ_int + "," + Alt_int + "," + VOC_int + "," + Temp_int + "," + Humid_int);
                    
                    //counter++;
                    //string receivedData = Encoding.UTF8.GetString(buffer);
                    //Console.WriteLine(receivedData);
                    //TMData.Enqueue(receivedData);
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
