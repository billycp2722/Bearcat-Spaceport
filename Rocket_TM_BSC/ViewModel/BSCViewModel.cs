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
