using JetBrains.Annotations;
using SciChart.Core.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static xqp;

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
            //Console.WriteLine(StringList.Length);
            //Console.WriteLine(StringList[13]);
            if (StringList.Length == 14)
            {

                double Lat_int = ConvertFromHex(StringList[0]).ToDouble(); // Has +/-
                double Lon_int = ConvertFromHex(StringList[1]).ToDouble(); // Has +/-
                double MS_int = ConvertFromHex(StringList[2]).ToDouble();
                double SatCoun_int = ConvertFromHex(StringList[3]).ToDouble();
                double GyroX_int = ConvertFromHex(StringList[4]).ToDouble(); // Has +/-
                double GyroY_int = ConvertFromHex(StringList[5]).ToDouble(); // Has +/-
                double GyroZ_int = ConvertFromHex(StringList[6]).ToDouble(); // Has +/-
                double AccelX_int = ConvertFromHex(StringList[7]).ToDouble();// Has +/-
                double AccelY_int = ConvertFromHex(StringList[8]).ToDouble();// Has +/-
                double AccelZ_int = ConvertFromHex(StringList[9]).ToDouble();// Has +/-
                double Alt_int = ConvertFromHex(StringList[10]).ToDouble(); // Has +/-
                double VOC_int = ConvertFromHex(StringList[11]).ToDouble();
                double Temp_int = ConvertFromHex(StringList[12]).ToDouble(); // Has +/-

                string replacement = Regex.Replace(StringList[13], @"\t|\n|\r", "");
                double Humid_int = ConvertFromHex(replacement).ToDouble();

                Lat_int = Lat_int * 0.0000001;
                Lon_int = Lon_int * 0.0000001;
                AccelX_int = AccelX_int * 0.01; // M/s^2
                AccelY_int = AccelY_int * 0.01; // M/s^2
                AccelZ_int = AccelY_int * 0.01; // M/s*^2

                // Need to convert Time somehow
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
            //Console.WriteLine(HexValue);
            try
            {
                if (HexValue.StartsWith("+"))
                {

                    //Console.WriteLine("1:" + HexValue);
                    int val = Convert.ToInt32(HexValue, 16);
                    output = val.ToString();
                }
                else if (HexValue.StartsWith("-"))
                {

                    string tmp = HexValue.Substring(1);
                    int val = Convert.ToInt32(tmp, 16);
                    val = -val;
                    output = val.ToString();
                }
                else
                {
                    //Console.WriteLine("2:" + HexValue);
                    int val = Convert.ToInt32(HexValue, 16);

                    output = val.ToString();
                }
            }


            //if (HexValue.Length == 1)
            //{
            //    // Convert the single character to its ASCII value
            //    int val = (int)HexValue[0];
            //    output = val.ToString();
            //}


            catch
            {

            }

            return output;
        }

        private bool IsHexString(string hexString)
        {
            // Regular expression to match hexadecimal string
            string pattern = "^[0-9A-Fa-f]+$";
            return Regex.IsMatch(hexString, pattern);
        }
    }
}
