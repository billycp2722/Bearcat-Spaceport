using SciChart.Core.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Rocket_TM_BSC.Model
{
    public class Cap1_DataProcessing
    {
        private BackgroundWorker Cap1DataProcessor;
        public ConcurrentQueue<byte[]> Cap1DataQueue;
        public ConcurrentQueue<double[]> Cap1_DataOut;
        public Cap1_DataProcessing()
        {
            Cap1DataProcessor = new BackgroundWorker();
            Cap1DataProcessor.DoWork += Cap1DataProcessor_DoWork;
            Cap1DataProcessor.RunWorkerCompleted += Cap1DataProcessor_RunWorkerCompleted;
            Cap1DataProcessor.ProgressChanged += Cap1DataProcessor_ProgressChanged;
            Cap1DataProcessor.WorkerSupportsCancellation = true;
            Cap1_DataOut = new ConcurrentQueue<double[]>();
            Cap1DataQueue = new ConcurrentQueue<byte[]>();
        }

        
        private void Cap1DataProcessor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }
        private void Cap1DataProcessor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }
        //Stopwatch stopwatch = new Stopwatch();
        int j = 1;
        private void Cap1DataProcessor_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                //stopwatch.Start();
                while (Cap1DataQueue.Count > 0)
                {
                    try
                    {
                        
                        Cap1DataQueue.TryDequeue(out var data);
                        ConvertBytes(data);

                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message.ToString()); }
                    
                }
                
                //Console.WriteLine(stopwatch.ElapsedMilliseconds + ": " +j);
                //j++;
                //stopwatch.Reset();

            }


        }
        public void StartCap1DataProcess()
        {
            Cap1DataProcessor.RunWorkerAsync();
        }
        public void ConvertBytes(byte[] buffer)
        {
            byte[] Lat = new byte[4] { buffer[0], buffer[1], buffer[2], buffer[3] };
            byte[] Lon = new byte[4] { buffer[4], buffer[5], buffer[6], buffer[7] };
            byte[] MS = new byte[4] { buffer[8], buffer[9], buffer[10], buffer[11] };
            byte SatCoun = buffer[12];
            byte GyroX = buffer[13];
            byte GyroY = buffer[14];
            byte GyroZ = buffer[15];
            byte[] AccelX = new byte[2] { buffer[16], buffer[17] };
            byte[] AccelY = new byte[2] { buffer[18], buffer[19] };
            byte[] AccelZ = new byte[2] { buffer[20], buffer[21] };
            byte[] Alt = new byte[2] { buffer[22], buffer[23] };
            byte[] VOC = new byte[2] { buffer[24], buffer[25] };
            byte[] Temp = new byte[2] { buffer[26], buffer[27] };
            byte Humid = buffer[28];



            double Lat_int = BitConverter.ToInt32(Lat, 0);
            double Lon_int = BitConverter.ToInt32(Lon, 0);
            double MS_int = BitConverter.ToUInt32(MS, 0);
            double SatCoun_int = SatCoun.ToDouble();
            double GyroX_int = GyroX.ToDouble();
            double GyroY_int = GyroY.ToDouble();
            double GyroZ_int = GyroZ.ToDouble();
            double AccelX_int = BitConverter.ToInt16(AccelX, 0);
            double AccelY_int = BitConverter.ToInt16(AccelY, 0);
            double AccelZ_int = BitConverter.ToInt16(AccelZ, 0);
            double Alt_int = BitConverter.ToUInt16(Alt, 0);
            double VOC_int = BitConverter.ToUInt16(VOC, 0);
            double Temp_int = BitConverter.ToInt16(Temp, 0);
            double Humid_int = Humid.ToDouble();

            double[] Cap1DataOut = new double[14] {Lat_int,Lon_int,MS_int,SatCoun_int,GyroX_int, GyroY_int, GyroZ_int, AccelX_int, AccelY_int, AccelZ_int, Alt_int, VOC_int, Temp_int, Humid_int};
            //string output = "";

            //Console.WriteLine(Cap1DataOut[0] + "," + Cap1DataOut[1] + "," + Cap1DataOut[2] + "," + Cap1DataOut[3] + "," + Cap1DataOut[4] + "," + Cap1DataOut[5] + "," + Cap1DataOut[6] + "," + Cap1DataOut[7] + "," + Cap1DataOut[8] + "," + Cap1DataOut[9] + "," + Cap1DataOut[10] + "," + Cap1DataOut[11] + "," + Cap1DataOut[12] + "," + Cap1DataOut[13]);
            Cap1_DataOut.Enqueue(Cap1DataOut);

        }
    }
}
