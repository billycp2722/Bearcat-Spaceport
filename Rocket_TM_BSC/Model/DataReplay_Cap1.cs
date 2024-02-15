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
    public class DataReplay_Cap1
    {
        private BackgroundWorker DataWorker;
        public DataReplay_Cap1()
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
        public void RunDataPlayback(string filename)
        {
            filepath = filename;
            DataWorker.RunWorkerAsync();
        }

        public void RunDataPlayback(string filename, string filename2)
        {
            filepath = filename;
            DataWorker.RunWorkerAsync();
        }

        private void DataWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            LoadCsv(filepath);
            DisplayData();
        }
        // All
        private double[] DP1; // Lat
        private double[] DP2; // Lon
        private double[] DP3; // gpsAlt G7
        private double[] DP4; // satCount G8
        private double[] DP5; // timestamp // Odd Formating, no zero padding
        private double[] DP6; // accelX G9
        private double[] DP7; // accelY
        private double[] DP8; // accelZ
        private double[] DP9; // baroAlt G7

        // Atmos Capsule
        private double[] DP10; // VOC G10
        private double[] DP11; // Humid G11
        private double[] DP12; // Temp G12
        private double[] DP13; // gyroX G13
        private double[] DP14; // gyroY
        private double[] DP15; // gyroZ

        // Graph Count 1 2 3 4 5 6 7 8 9
        // Graph 10, velocity magnitudes

        public ConcurrentQueue<double[]> Cap1Replay;
        public ConcurrentQueue<double[]> Cap2Replay;
        private void LoadCsv (string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                long x = new FileInfo(filename).Length;
                
                int FileLen = Convert.ToInt32(x);
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

                for (int i = 1; i < FileLen; i++)
                {
                    string s = reader.ReadLine();
                    if (s == null)
                    {
                        break;
                    }
                    string[] values = s.Split(',').Select(sValue => sValue.Trim()).ToArray();

                    if (i > 0)
                    {
                        
                        DP1[1] = (double.Parse(values[1], NumberStyles.Float));
                        DP2[i] = (double.Parse(values[2], NumberStyles.Float));
                        DP3[i] = (double.Parse(values[3], NumberStyles.Float));
                        DP4[i] = (double.Parse(values[4], NumberStyles.Float));
                        DP5[i] = (double.Parse(values[5], NumberStyles.Float));
                        DP6[i] = (double.Parse(values[6], NumberStyles.Float));
                        DP7[i] = (double.Parse(values[7], NumberStyles.Float));
                        DP8[i] = (double.Parse(values[8], NumberStyles.Float));
                        DP9[i] = (double.Parse(values[9], NumberStyles.Float));
                        // Continue adding as needed
                        if (values.Length > 9)
                        {
                            DP10[i] = (double.Parse(values[10], NumberStyles.Float));
                            DP11[i] = (double.Parse(values[11], NumberStyles.Float));
                            DP12[i] = (double.Parse(values[12], NumberStyles.Float));
                            DP13[i] = (double.Parse(values[13], NumberStyles.Float));
                            DP14[i] = (double.Parse(values[14], NumberStyles.Float));
                            DP15[i] = (double.Parse(values[15], NumberStyles.Float));
                        }
                    }
                    
                }
            }
            
        }

        private void DisplayData()
        {
            Cap1Replay = new ConcurrentQueue<double[]>();
            Cap2Replay = new ConcurrentQueue<double[]>();
           for (int i = 0; i < DP1.Length; i++)
            {
                double[] c1ReplayOut = new double[15] { DP1[i], DP2[i], DP3[i], DP4[i], DP5[i], DP6[i], DP7[i], DP8[i], DP9[i], DP10[i], DP11[i], DP12[i], DP13[i], DP14[i], DP15[i] };
                Cap1Replay.Enqueue(c1ReplayOut);
                // Use concurrent queues to send data to GUI for updating on specical timer tick
                // Do we try to time sync? Don't know how to do that. GPS time polling is limited, maybe add a delta t to csv data logger
            }
        }
    }
}
