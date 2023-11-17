using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            try
            {
                string data = _serialport.ReadLine();
                string[] values = data.Split(',');

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
            catch
            {

            }
        }

        private void TMDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Read Serial data and return the data to a 
        }

        private SerialPort _serialport;
        
        public void OpenNewPort(string COM, int Baud, Parity parity, int dataBits, StopBits stopBits)
        {
            try
            {
                _serialport = new SerialPort(COM, Baud, parity, dataBits, stopBits);
                _serialport.Open();

            }
            catch (Exception ex)
            {
                return;
            }

            TMDataWorker.RunWorkerAsync();
        }
    }
}
