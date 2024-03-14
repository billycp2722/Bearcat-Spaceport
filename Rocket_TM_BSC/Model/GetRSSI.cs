using SciChart.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_TM_BSC.Model
{
    public class GetRSSI
    {
        private BackgroundWorker RSSIDataWorker;
        public GetRSSI()
        {
            RSSIDataWorker = new BackgroundWorker();
            RSSIDataWorker.DoWork += RSSIDataWorker_DoWork;
            RSSIDataWorker.RunWorkerCompleted += RSSIDataWorker_RunWorkerCompleted;
            RSSIDataWorker.ProgressChanged += RSSIDataWorker_ProgressChanged;
            RSSIDataWorker.WorkerSupportsCancellation = true;
        }

        private void RSSIDataWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }
        private void RSSIDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private string comport;
        private SerialPort _serialport;
        public double RSSI_Cap1 = 0;
        public double RSSI_Cap2 = 0;
        public double RSSI_Rocket = 0;
        private void RSSIDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _serialport = new SerialPort(comport, 9600, Parity.None, 8, StopBits.One);
            _serialport.Open();
            _serialport.DiscardNull = true;
            _serialport.DiscardInBuffer();

            while (_serialport.IsOpen)
            {
               
                int bytesToRead = 50;
                byte[] buffer = new byte[bytesToRead];
                int bytesRead = 0;
                int LastByte = 0;
                while (bytesRead < bytesToRead)
                   
                {
                    //Console.WriteLine(bytesRead);
                    try
                    {
                        bytesRead += _serialport.BaseStream.Read(buffer, bytesRead, bytesToRead - bytesRead);
                        if ((char)buffer[bytesRead - 1] == '\n')
                        {
                            LastByte = bytesRead-1;
                            break;
                        }
                    }
                    catch
                    {
                    }

                }

                byte[] Tmpbuffer = buffer.Take(LastByte).ToArray();
                string RSSIData = Encoding.UTF8.GetString(Tmpbuffer);
                string[] StringList = RSSIData.Split(',');
                try
                {
                    RSSI_Cap1 = (StringList[0]).ToDouble();
                    RSSI_Cap2 = (StringList[1]).ToDouble();
                    RSSI_Rocket = (StringList[2]).ToDouble();
                }
                catch
                {

                }
                

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
            RSSIDataWorker.RunWorkerAsync();
        }
    }
}
