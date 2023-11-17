using SciChart.Examples.ExternalDependencies.Common;
using Rocket_TM_BSC.Commands;
using Rocket_TM_BSC.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SciChart.Charting3D;
using SciChart.Charting3D.Visuals.Object;
using System.Security.AccessControl;
using System.IO;
using SciChart.Charting.Visuals;
using SciChart.Charting3D.Model;
using System.Windows.Controls;
using System.Drawing;

namespace Rocket_TM_BSC.ViewModel
{
    public class BSCViewModel : BaseViewModel
    {
        #region Variables

        public ViewCommand TestCommand { get; set; }
        
        #endregion
        public BSCViewModel()
        {
            TestCommand = new ViewCommand(Test, CanTest);
            
        }

        #region Public Bindings
        private Rotation3D rock = new Rotation3D();

        public Rotation3D RotateRocket
        {
            get { return rock; }
            set { rock = value; OnPropertyChanged("RotateRocket"); }
        }

        private string rocketposition = "10,20,0";
        public string RocketPosition
        {
            get { return rocketposition; }
            set { rocketposition = value; OnPropertyChanged("RocketPosition"); }
        }

        private string rocketSigStrength = null;
        public string RocketSigStrength
        {
            get { return rocketSigStrength; }
            set { rocketSigStrength = value; OnPropertyChanged("RocketSigStrength"); }
        }

        private string cap1Strength = null;
        public string Cap1Strength
        {
            get { return cap1Strength; }
            set { cap1Strength = value; OnPropertyChanged("Cap1Strength"); }
        }

        private string cap2Strength = null;
        public string Cap2Strength
        {
            get { return cap2Strength; }
            set { cap2Strength = value; OnPropertyChanged("Cap2Strength"); }
        }

        private Brush rockTMStat = Brushes.Red;
        public Brush RockTMStat
        {
            get { return rockTMStat; }
            set { rockTMStat = value; OnPropertyChanged("RockTMStat"); }
        }

        private Brush cap1TMStat = Brushes.Red;
        public Brush Cap1TMStat
        {
            get { return cap1TMStat; }
            set { cap1TMStat = value; OnPropertyChanged("Cap1TMStat"); }
        }

        private Brush cap2TMStat = Brushes.Red;
        public Brush Cap2TMStat
        {
            get { return cap2TMStat; }
            set { cap2TMStat = value; OnPropertyChanged("Cap2TMStat"); }
        }

        private Brush capEject = Brushes.Red;
        public Brush CapEject
        {
            get { return capEject; }
            set { capEject = value; OnPropertyChanged("CapEject"); }
        }

        private string rocketAlt = null;
        public string RocketAlt
        {
            get { return rocketAlt; }
            set { rocketAlt = value; OnPropertyChanged("RocketAlt"); }
        }

        private string apogeeAlt = null;
        public string ApogeeAlt
        {
            get { return apogeeAlt; }
            set { apogeeAlt = value; OnPropertyChanged(" ApogeeAlt"); }
        }

        private string sysPressure1 = null;
        public string SysPressure1
        {
            get { return sysPressure1; }
            set { sysPressure1 = value; OnPropertyChanged(" SysPressure1"); }
        }

        private string sysPressure2 = null;
        public string SysPressure2
        {
            get { return sysPressure2; }
            set { sysPressure2 = value; OnPropertyChanged(" SysPressure2"); }
        }

        private string cap1_SatCount= "0";
        public string Cap1_SatCount
        {
            get { return cap1_SatCount; }
            set { cap1_SatCount = value; OnPropertyChanged("Cap1_SatCount"); }
        }

        private string cap2_SatCount = "0";
        public string Cap2_SatCount
        {
            get { return cap2_SatCount; }
            set { cap2_SatCount = value; OnPropertyChanged("Cap2_SatCount"); }
        }

        private string cap1_GPSLat = null;
        public string Cap1_GPSLat
        {
            get { return cap1_GPSLat; }
            set { cap1_GPSLat = value; OnPropertyChanged("Cap1_GPSLat"); }
        }

        private string cap2_GPSLat = null;
        public string Cap2_GPSLat
        {
            get { return cap2_GPSLat; }
            set { cap2_GPSLat = value; OnPropertyChanged("Cap2_GPSLat"); }
        }

        private string cap1_GPSLon = null;
        public string Cap1_GPSLon
        {
            get { return cap1_GPSLon; }
            set { cap1_GPSLon = value; OnPropertyChanged("Cap1_GPSLon"); }
        }

        private string cap2_GPSLon = null;
        public string Cap2_GPSLon
        {
            get { return cap2_GPSLon; }
            set { cap2_GPSLon = value; OnPropertyChanged("Cap2_GPSLon"); }
        }

        private string cap1_Alt = "0";
        public string Cap1_Alt
        {
            get { return cap1_Alt; }
            set { cap1_Alt = value; OnPropertyChanged("Cap1_Alt "); }
        }

        private string cap2_Alt = "0";
        public string Cap2_Alt
        {
            get { return cap2_Alt; }
            set { cap2_Alt = value; OnPropertyChanged("Cap2_Alt "); }
        }

        #endregion


        public void Test(object obj)
        {
            

            //throw new NotImplementedException();
        }

        public bool CanTest(object obj)
        {
            return true;
        }

        #region SciChartTests
        public static Stream StringToStream(string src)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(src);
            return new MemoryStream(byteArray);
        }
        #endregion
    }
}
