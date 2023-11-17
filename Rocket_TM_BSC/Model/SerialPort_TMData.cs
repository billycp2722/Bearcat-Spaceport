using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Rocket_TM_BSC.Model
{
    public class SerialPort_TMData
    {

        public SerialPort_TMData()
        {
            
        }

        OpenNewPort(string COM, int Baud, Parity parity, int dataBits, StopBits stopBits)
        {
            _serialport = new SerialPort();
        }
    }
}
