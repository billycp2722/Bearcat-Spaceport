using JetBrains.Annotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_TM_BSC.Model
{
    public class DataReplay_Cap2
    {
        private BackgroundWorker DataWorker;
        public DataReplay_Cap2()
        {
            DataWorker = new BackgroundWorker();
            DataWorker.DoWork += DataWorker_DoWork;
            DataWorker.RunWorkerCompleted += DataWorker_RunWorkerCompleted;
            DataWorker.ProgressChanged += DataWorker_ProgressChanged;
            DataWorker.WorkerSupportsCancellation = true;
        }

        private void DataWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }
        private void DataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private string filepath = "";
        private string filepath2 = "";
        public void RunDataPlayback(string filename)
        {
            filepath = filename;
            DataWorker.RunWorkerAsync();
        }

        public void RunDataPlayback(string filename, string filename2)
        {
            filepath = filename;
            filepath2 = filename2;
            DataWorker.RunWorkerAsync();
        }

        private void DataWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            LoadCsv(filepath);
            Replay_Cap2_Ready = true;
        }
        // All
        public double[] DP1; // Lat
        public double[] DP2; // Lon
        public double[] DP3; // gpsAlt G7
        public double[] DP4; // satCount G8
        public double[] DP5; // timestamp // Odd Formating, no zero padding
        public double[] DP6; // accelX G9
        public double[] DP7; // accelY
        public double[] DP8; // accelZ
        public double[] DP9; // baroAlt G7

        // Atmos Capsule
        public double[] DP10; // VOC G10
        public double[] DP11; // Humid G11
        public double[] DP12; // Temp G12
        public double[] DP13; // gyroX G13
        public double[] DP14; // gyroY
        public double[] DP15; // gyroZ

        public bool Replay_Cap2_Ready = false;

        // Graph Count 1 2 3 4 5 6 7 8 9
        // Graph 10, velocity magnitudes

        public ConcurrentQueue<double[]> Cap1Replay;
        public ConcurrentQueue<double[]> Cap2Replay;
        public double MaxAlt = 0;
        

        private void LoadCsv (string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                int FileLen = File.ReadAllLines(filename).Length;
                Console.WriteLine(FileLen);
                //int FileLen = Convert.ToInt32(x);
                DP1 = new double[FileLen];
                DP2 = new double[FileLen];
                DP3 = new double[FileLen];
                DP4 = new double[FileLen];
                DP5 = new double[FileLen];
                DP6 = new double[FileLen];
                DP7 = new double[FileLen];
                DP8 = new double[FileLen];
                DP9 = new double[FileLen];
                DP10 = new double[FileLen];
                DP11 = new double[FileLen];
                DP12 = new double[FileLen];
                DP13 = new double[FileLen];
                DP14 = new double[FileLen];
                DP15 = new double[FileLen];

                for (int i = 0; i < FileLen; i++)
                {
                    string s = reader.ReadLine();
                    if (s == null)
                    {
                        return;
                    }
                    string[] values = s.Split(',').Select(sValue => sValue.Trim()).ToArray();
                    //Console.WriteLine(values[0]);
                    if (i > 0)
                    {
                        
                        DP1[i-1] = (double.Parse(values[0], NumberStyles.Float));
                        DP2[i-1] = (double.Parse(values[1], NumberStyles.Float));
                        DP3[i-1] = (double.Parse(values[2], NumberStyles.Float));
                        DP4[i-1] = (double.Parse(values[3], NumberStyles.Float));
                        //DP5[i-1] = (double.Parse(values[4], NumberStyles.Float));
                        DP6[i-1] = (double.Parse(values[5], NumberStyles.Float));
                        DP7[i-1] = (double.Parse(values[6], NumberStyles.Float));
                        DP8[i-1] = (double.Parse(values[7], NumberStyles.Float));
                        DP9[i-1] = (double.Parse(values[8], NumberStyles.Float));
                        // Continue adding as needed
                        if (values.Length > 9)
                        {
                            DP10[i-1] = (double.Parse(values[9], NumberStyles.Float));
                            //DP11[i-1] = (double.Parse(values[10], NumberStyles.Float)); // Voc
                            //DP12[i-1] = (double.Parse(values[11], NumberStyles.Float)); // Tmp
                            //DP13[i-1] = (double.Parse(values[12], NumberStyles.Float)); // Humid
                            DP14[i-1] = (double.Parse(values[10], NumberStyles.Float));
                            DP15[i-1] = (double.Parse(values[11], NumberStyles.Float));
                        }
                    }
                    
                }
            }
            MaxAlt = DP9.Max();
        }

        private void DisplayData()
        {
            
           
        }
    }
}
