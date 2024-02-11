using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_TM_BSC.Model
{
    public class AccelToVelo
    {
        public AccelToVelo()
        {
            
        }

        public double[] VelocityFromAcceleration(double[] accel)
        {
            double accel_x = accel[0];
            double accel_y = accel[1];
            double accel_z = accel[2];

            double veloX = 0;
            double veloY = 0;
            double veloZ = 0;

            double[] Velocity = new double[3] { veloX, veloY, veloZ }; // Return components or Magnitude?
            return Velocity;
        }
    }
}
