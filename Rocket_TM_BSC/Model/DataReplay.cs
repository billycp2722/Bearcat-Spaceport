using System;
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
    public class DataReplay
    {
        private BackgroundWorker DataWorker;
        public DataReplay()
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

        private void DataWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            LoadCsv(filepath);
            DisplayData();
        }

        private double[] DP1;
        private double[] DP2;
        private double[] DP3;
        private double[] DP4;
        private double[] DP5;
        private double[] DP6;
        private double[] DP7;
        private double[] DP8;
        private double[] DP9;
        private double[] DP10;
        // Add more as needed 
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

                for (int i = 0; i < FileLen; i++)
                {
                    string s = reader.ReadLine();
                    if (s == null)
                    {
                        break;
                    }
                    string[] values = s.Split(',').Select(sValue => sValue.Trim()).ToArray();
                    if (i > 0)
                    {
                        
                        DP1[i] = (double.Parse(values[1], NumberStyles.Float));
                        DP2[i] = (double.Parse(values[2], NumberStyles.Float));
                        DP3[i] = (double.Parse(values[3], NumberStyles.Float));
                        DP4[i] = (double.Parse(values[3], NumberStyles.Float));
                        DP5[i] = (double.Parse(values[3], NumberStyles.Float));
                        DP6[i] = (double.Parse(values[3], NumberStyles.Float));
                        DP7[i] = (double.Parse(values[3], NumberStyles.Float));
                        DP8[i] = (double.Parse(values[3], NumberStyles.Float));
                        DP9[i] = (double.Parse(values[3], NumberStyles.Float));
                        // Continue adding as needed
                    }
                }
            }
            
        }

        private void DisplayData()
        {
           for (int i = 0; i < DP1.Length; i++)
            {
                // Use concurrent queues to send data to GUI for updating on specical timer tick
                // Do we try to time sync? Don't know how to do that. GPS time polling is limited, maybe add a delta t to csv data logger
            }
        }
    }
}
