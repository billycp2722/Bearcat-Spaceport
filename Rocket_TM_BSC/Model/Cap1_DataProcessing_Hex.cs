using JetBrains.Annotations;
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
    public class Cap1_DataProcessing_Hex
    {
        private BackgroundWorker Cap1DataProcessor;
        public ConcurrentQueue<byte[]> Cap1DataQueue_Hex;
        public ConcurrentQueue<double[]> Cap1_DataOut_Hex;
        public Cap1_DataProcessing_Hex()
        {
            Cap1DataProcessor = new BackgroundWorker();
            Cap1DataProcessor.DoWork += Cap1DataProcessor_DoWork;
            Cap1DataProcessor.RunWorkerCompleted += Cap1DataProcessor_RunWorkerCompleted;
            Cap1DataProcessor.ProgressChanged += Cap1DataProcessor_ProgressChanged;
            Cap1DataProcessor.WorkerSupportsCancellation = true;
            Cap1_DataOut_Hex = new ConcurrentQueue<double[]>();
            Cap1DataQueue_Hex = new ConcurrentQueue<byte[]>();
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
                while (Cap1DataQueue_Hex.Count > 0)
                {
                    try
                    {
                        
                        Cap1DataQueue_Hex.TryDequeue(out var data);
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
            string ConvertedString = Encoding.UTF8.GetString(buffer);
            string[] StringList = ConvertedString.Split(',');
            if (StringList.Length == 14)
            {

                double Lat_int = StringList[0].ToDouble(); // Has +/-
                
                double Lon_int = StringList[1].ToDouble(); // Has +/-
                double MS_int = StringList[2].ToDouble();
                double SatCoun_int = StringList[3].ToDouble();
                double GyroX_int = StringList[4].ToDouble(); // Has +/-
                double GyroY_int = StringList[5].ToDouble(); // Has +/-
                double GyroZ_int = StringList[6].ToDouble(); // Has +/-
                double AccelX_int = StringList[7].ToDouble();// Has +/-
                double AccelY_int = StringList[8].ToDouble();// Has +/-
                double AccelZ_int = StringList[9].ToDouble();// Has +/-
                double Alt_int = StringList[10].ToDouble(); // Has +/-
                double VOC_int = StringList[11].ToDouble(); 
                double Temp_int = StringList[12].ToDouble(); // Has +/-
                double Humid_int = StringList[13].ToDouble();



                double[] Cap1DataOut = new double[14] { Lat_int, Lon_int, MS_int, SatCoun_int, GyroX_int, GyroY_int, GyroZ_int, AccelX_int, AccelY_int, AccelZ_int, Alt_int, VOC_int, Temp_int, Humid_int };
                //string output = "";

                //Console.WriteLine(Cap1DataOut[0] + "," + Cap1DataOut[1] + "," + Cap1DataOut[2] + "," + Cap1DataOut[3] + "," + Cap1DataOut[4] + "," + Cap1DataOut[5] + "," + Cap1DataOut[6] + "," + Cap1DataOut[7] + "," + Cap1DataOut[8] + "," + Cap1DataOut[9] + "," + Cap1DataOut[10] + "," + Cap1DataOut[11] + "," + Cap1DataOut[12] + "," + Cap1DataOut[13]);
                Cap1_DataOut_Hex.Enqueue(Cap1DataOut);
            }
            else
            {
                Console.WriteLine("String List Incorrect Size");
            }
        }

        public string ConvertFromHex(string HexValue)
        {
            string output = "";
            try
            {
                if (HexValue.StartsWith("+") || HexValue.StartsWith("-"))
                {
                    char index = HexValue.First();
                    string tmp = HexValue;
                }
                else
                {

                }

            }
            catch
            {

            }

            return output;
        }
    }
}
