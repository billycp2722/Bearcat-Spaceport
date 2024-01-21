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
using System.Windows.Media;
using System.Windows.Threading;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using System.Collections.ObjectModel;
using System.IO.Ports;
using SciChart.Core.Extensions;

namespace Rocket_TM_BSC.ViewModel
{

    public class BSCViewModel : BaseViewModel
    {
        #region Variables

        // Button Commands
        public ViewCommand TestCommand { get; set; }
        public ViewCommand TMCap1OpenCommand { get; set; }
        public ViewCommand TMCap2OpenCommand { get; set; }
        public ViewCommand TMRocketOpenCommand { get; set; }
        public ViewCommand UpdateComPortCommand { get; set; }
        public ViewCommand OpenRocketCOMCommand { get; set; }
        public ViewCommand OpenCap1COMCommand { get; set; }
        public ViewCommand OpenCap2COMCommand { get; set; }
        public ViewCommand StatusCheckRocket { get; set; }
        public ViewCommand StatusCheckCap1 { get; set; }
        public ViewCommand StatusCheckCap2 { get; set; }
        public ViewCommand WakeRocket { get; set; }
        public ViewCommand WakeCap1 { get; set; }
        public ViewCommand WakeCap2 { get; set; }

        // Class Instances
        private DispatcherTimer _timer;
        private Cap1_TM Cap1_Data = new Cap1_TM();
        private Cap1_TM Cap2_Data = new Cap1_TM();
        private Cap1_TM Rocket_Data = new Cap1_TM();

        // GUI Variables
        private string rocketposition = "10,20,0";
        private string rocketSigStrength = null;
        private string cap1Strength = null;
        private string cap2Strength = null;
        private Brush rockTMStat = Brushes.Red;
        private Brush cap1TMStat = Brushes.Red;
        private Brush cap2TMStat = Brushes.Red;
        private Brush capEject = Brushes.Red;
        private Brush cap1_ParachuteDep = Brushes.Red;
        private Brush cap2_ParachuteDep = Brushes.Red;
        private string rocketAlt = null;
        private string apogeeAlt = null;
        private string sysPressure1 = null;
        private string sysPressure2 = null;
        private string cap1_SatCount = "0";
        private string cap2_SatCount = "0";
        private string cap1_GPSLat = null;
        private string cap2_GPSLat = null;
        private string cap1_GPSLon = null;
        private string cap2_GPSLon = null;
        private string cap1_Alt = "0";
        private string cap2_Alt = "0";
        private string cap1_Velo = null;
        private string cap2_Velo = null;

        #endregion

        // Constructor
        public BSCViewModel()
        {
            TestCommand = new ViewCommand(Test, CanTest);
            TMCap1OpenCommand = new ViewCommand(TMCap1Open, CanTMCap1Open);
            TMCap2OpenCommand = new ViewCommand(TMCap2Open, CanTMCap2Open);
            TMRocketOpenCommand = new ViewCommand(TMRocketOpen, CanTMRocketOpen);
            UpdateComPortCommand = new ViewCommand(UpdateComPort, CanUpdateComPort);
            OpenRocketCOMCommand = new ViewCommand(OpenRocketCOM, CanOpenRocketCOM);
            OpenCap1COMCommand = new ViewCommand(OpenCap1COM, CanOpenCap1COM);
            OpenCap2COMCommand = new ViewCommand(OpenCap2COM, CanOpenCap2COM);

            StatusCheckRocket = new ViewCommand(StatCheckRocket, CanStatCheckRocket);
            StatusCheckCap1 = new ViewCommand(StatCheckCap1, CanStatCheckCap1);
            StatusCheckCap2 = new ViewCommand(StatCheckCap1, CanStatCheckCap2);

            WakeRocket = new ViewCommand(WakeUpRocket, CanWakeUpRocket);
            WakeCap1 = new ViewCommand(WakeUpCap1, CanWakeUpCap1);
            WakeCap2 = new ViewCommand(WakeUpCap2, CanWakeUpCap2);
            



            InitializeGraph();
            dataSeriesCap1G1.AcceptsUnsortedData = true; // Alt graph
            dataSeriesCap1G2.AcceptsUnsortedData = true; // Velocity Graph
            dataSeriesCap1G3.AcceptsUnsortedData = true; // Temp / Humidity Graph
            dataSeriesCap1G4.AcceptsUnsortedData = true; // VOC Graph
            dataSeriesCap1G5.AcceptsUnsortedData = true; // Satalite Count Graph

            dataSeriesCap2G1.AcceptsUnsortedData = true; // Alt graph
            dataSeriesCap2G2.AcceptsUnsortedData = true; // Velocity Graph
            dataSeriesCap2G3.AcceptsUnsortedData = true; // Temp / Humidity Graph
            dataSeriesCap2G4.AcceptsUnsortedData = true; // VOC Grap
            dataSeriesCap2G5.AcceptsUnsortedData = true; // Satalite Count Graph

            dataSeriesRocketG1.AcceptsUnsortedData = true; // Alt graph
            dataSeriesRocketG2.AcceptsUnsortedData = true;
            dataSeriesRocketG3.AcceptsUnsortedData = true;
            dataSeriesRocketG4.AcceptsUnsortedData = true;
            dataSeriesRocketG5.AcceptsUnsortedData = true;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(5);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            UpdateComPortCommand.Execute(this);




        }
        private int i = 1;
        private void _timer_Tick(object sender, EventArgs e)
        {
            if (RocketAlt != Rocket_Data.Data1) { RocketAlt = Rocket_Data.Data1; dataSeriesCap1G1.Append(i, Rocket_Data.Data1.ToDouble()); i++; }
            if (ApogeeAlt != Rocket_Data.Data2) { ApogeeAlt = Rocket_Data.Data2; }
            if (Cap1_Alt != Rocket_Data.Data3) { Cap1_Alt = Rocket_Data.Data3; }
            if (Cap2_Alt != Rocket_Data.Data4) { Cap2_Alt = Rocket_Data.Data4; }
            if (Cap1_SatCount != Rocket_Data.Data5) { Cap1_SatCount = Rocket_Data.Data5; }

            //dataSeriesCap1G1.Append(i, Rocket_Data.Data1.ToDouble());
            while (Rocket_Data.TMData.Count > 0)
            {
                try
                {
                    Rocket_Data.TMData.TryDequeue(out string tm);
                    string[] tmp = tm.Split(',');
                    dataSeriesCap1G1.Append(i, tmp[0].ToDouble());
                    dataSeriesCap1G2.Append(i, tmp[1].ToDouble());
                    dataSeriesCap1G3.Append(i, tmp[2].ToDouble());
                    dataSeriesCap1G5.Append(i, i);
                    i++;
                }
                catch
                {

                }
            }
            
        }

        #region Public Bindings
        public string RocketPosition
        {
            get { return rocketposition; }
            set { rocketposition = value; OnPropertyChanged("RocketPosition"); }
        }

        
        public string RocketSigStrength
        {
            get { return rocketSigStrength; }
            set { rocketSigStrength = value; OnPropertyChanged("RocketSigStrength"); }
        }

        
        public string Cap1Strength
        {
            get { return cap1Strength; }
            set { cap1Strength = value; OnPropertyChanged("Cap1Strength"); }
        }

        
        public string Cap2Strength
        {
            get { return cap2Strength; }
            set { cap2Strength = value; OnPropertyChanged("Cap2Strength"); }
        }

        
        public Brush RockTMStat
        {
            get { return rockTMStat; }
            set { rockTMStat = value; OnPropertyChanged("RockTMStat"); }
        }

        
        public Brush Cap1TMStat
        {
            get { return cap1TMStat; }
            set { cap1TMStat = value; OnPropertyChanged("Cap1TMStat"); }
        }

        
        public Brush Cap2TMStat
        {
            get { return cap2TMStat; }
            set { cap2TMStat = value; OnPropertyChanged("Cap2TMStat"); }
        }

        
        public Brush CapEject
        {
            get { return capEject; }
            set { capEject = value; OnPropertyChanged("CapEject"); }
        }

        
        public Brush Cap1_ParachuteDep
        {
            get { return cap1_ParachuteDep; }
            set { cap1_ParachuteDep = value; OnPropertyChanged("Cap1_ParachuteDep"); }
        }

        
        public Brush Cap2_ParachuteDep
        {
            get { return cap2_ParachuteDep; }
            set { cap2_ParachuteDep = value; OnPropertyChanged("Cap2_ParachuteDep"); }
        }

        
        public string RocketAlt
        {
            get { return rocketAlt; }
            set { rocketAlt = value; OnPropertyChanged("RocketAlt"); }
        }

        
        public string ApogeeAlt
        {
            get { return apogeeAlt; }
            set { apogeeAlt = value; OnPropertyChanged(" ApogeeAlt"); }
        }

        
        public string SysPressure1
        {
            get { return sysPressure1; }
            set { sysPressure1 = value; OnPropertyChanged(" SysPressure1"); }
        }

        
        public string SysPressure2
        {
            get { return sysPressure2; }
            set { sysPressure2 = value; OnPropertyChanged(" SysPressure2"); }
        }

        
        public string Cap1_SatCount
        {
            get { return cap1_SatCount; }
            set { cap1_SatCount = value; OnPropertyChanged("Cap1_SatCount"); }
        }

        
        public string Cap2_SatCount
        {
            get { return cap2_SatCount; }
            set { cap2_SatCount = value; OnPropertyChanged("Cap2_SatCount"); }
        }

       
        public string Cap1_GPSLat
        {
            get { return cap1_GPSLat; }
            set { cap1_GPSLat = value; OnPropertyChanged("Cap1_GPSLat"); }
        }

        
        public string Cap2_GPSLat
        {
            get { return cap2_GPSLat; }
            set { cap2_GPSLat = value; OnPropertyChanged("Cap2_GPSLat"); }
        }

        
        public string Cap1_GPSLon
        {
            get { return cap1_GPSLon; }
            set { cap1_GPSLon = value; OnPropertyChanged("Cap1_GPSLon"); }
        }

        
        public string Cap2_GPSLon
        {
            get { return cap2_GPSLon; }
            set { cap2_GPSLon = value; OnPropertyChanged("Cap2_GPSLon"); }
        }

        
        public string Cap1_Alt
        {
            get { return cap1_Alt; }
            set { cap1_Alt = value; OnPropertyChanged("Cap1_Alt"); }
        }

        
        public string Cap2_Alt
        {
            get { return cap2_Alt; }
            set { cap2_Alt = value; OnPropertyChanged("Cap2_Alt"); }
        }

        
        public string Cap1_Velo
        {
            get { return cap1_Velo; }
            set { cap1_Velo = value; OnPropertyChanged("Cap1_Velo"); }
        }

        
        public string Cap2_Velo
        {
            get { return cap2_Velo; }
            set { cap2_Velo = value; OnPropertyChanged("Cap2_Velo"); }
        }

        private ObservableCollection<string> rocketCOMPortList = new ObservableCollection<string>();
        public ObservableCollection<string> RocketCOMPortList
        {
            get { return rocketCOMPortList; }
            set
            {
                if (rocketCOMPortList != value)
                {
                    rocketCOMPortList = value;
                    OnPropertyChanged(nameof(RocketCOMPortList));
                }
            }
        }

        private string rocketCOM = null;
        public string RocketCOM
        {
            get { return rocketCOM; }
            set { rocketCOM = value; OnPropertyChanged("RocketCOM"); }
        }

        private string cap1COM = null;
        public string Cap1COM
        {
            get { return cap1COM; }
            set { cap1COM = value; OnPropertyChanged("Cap1COM"); }
        }

        private string cap2COM = null;
        public string Cap2COM
        {
            get { return cap2COM; }
            set { cap2COM = value; OnPropertyChanged("Cap2COM"); }
        }

        #endregion

        #region Commands
        public void Test(object obj)
        {

            //throw new NotImplementedException();
        }

        public bool CanTest(object obj)
        {
            return true;
        }

        public void TMCap1Open(object obj)
        {

            //throw new NotImplementedException();
        }

        public bool CanTMCap1Open(object obj)
        {
            return true;
        }

        public void TMCap2Open(object obj)
        {

            //throw new NotImplementedException();
        }

        public bool CanTMCap2Open(object obj)
        {
            return true;
        }

        public void TMRocketOpen(object obj)
        {

            //throw new NotImplementedException();
        }

        public bool CanTMRocketOpen(object obj)
        {
            return true;
        }

        public void UpdateComPort(object obj)
        {

            //throw new NotImplementedException();
            RocketCOMPortList.Clear();
            string[] portNames = SerialPort.GetPortNames();
            if (portNames.Length == 0)
            {
                RocketCOMPortList.Add("NONE");
            }
            else
            {
                foreach (string portName in portNames)
                {
                    RocketCOMPortList.Add(portName);
                }
            }
            

        }

        public bool CanUpdateComPort(object obj)
        {
            return true;
        }

        public void OpenRocketCOM(object obj)
        {
            Console.WriteLine(RocketCOM);
            Rocket_Data.OpenNewPort(RocketCOM, 230400, Parity.None, 8, StopBits.One);
        }

        public bool CanOpenRocketCOM(object obj)
        {
            return true;
        }

        public void OpenCap1COM(object obj)
        {
            Console.WriteLine(RocketCOM);
            //Rocket_Data.OpenNewPort(RocketCOM, 230400, Parity.None, 8, StopBits.One);
        }

        public bool CanOpenCap1COM(object obj)
        {
            return true;
        }

        public void OpenCap2COM(object obj)
        {
            Console.WriteLine(RocketCOM);
            //Rocket_Data.OpenNewPort(RocketCOM, 230400, Parity.None, 8, StopBits.One);
        }

        public bool CanOpenCap2COM(object obj)
        {
            return true;
        }

        public void StatCheckRocket(object obj)
        {
            // Sends Status Check command to TM to update view
        }

        public bool CanStatCheckRocket(object obj)
        {
            return true;
        }

        public void StatCheckCap1(object obj)
        {
            // Sends Status Check command to TM to update view
        }

        public bool CanStatCheckCap1(object obj)
        {
            return true;
        }

        public void StatCheckCap2(object obj)
        {
            // Sends Status Check command to TM to update view
        }

        public bool CanStatCheckCap2(object obj)
        {
            return true;
        }

        public void WakeUpRocket(object obj)
        {
            // Sends Wake Command to TM to activate for launch
        }

        public bool CanWakeUpRocket(object obj)
        {
            return true;
        }

        public void WakeUpCap1(object obj)
        {
            // Sends Wake Command to TM to activate for launch
        }

        public bool CanWakeUpCap1(object obj)
        {
            return true;
        }

        public void WakeUpCap2(object obj)
        {
            // Sends Wake Command to TM to activate for launch
        }

        public bool CanWakeUpCap2(object obj)
        {
            return true;
        }



        #endregion

        #region SciChartTests
        public static Stream StringToStream(string src)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(src);
            return new MemoryStream(byteArray);
        }
        #endregion

        #region 2D Graphing
        
        private ObservableCollection<IRenderableSeriesViewModel> graph1Series { get; set; }
        private ObservableCollection<IRenderableSeriesViewModel> graph2Series { get; set; }
        private ObservableCollection<IRenderableSeriesViewModel> graph3Series { get; set; }
        private ObservableCollection<IRenderableSeriesViewModel> graph4Series { get; set; }
        private ObservableCollection<IRenderableSeriesViewModel> graph5Series { get; set; }
        public ObservableCollection<IRenderableSeriesViewModel> Graph1Series
        {
            get => graph1Series;
            set
            {
                graph1Series = value;
                OnPropertyChanged(nameof(Graph1Series));
            }
        }
        public ObservableCollection<IRenderableSeriesViewModel> Graph2Series
        {
            get => graph2Series;
            set
            {
                graph2Series = value;
                OnPropertyChanged(nameof(Graph2Series));
            }
        }
        public ObservableCollection<IRenderableSeriesViewModel> Graph3Series
        {
            get => graph3Series;
            set
            {
                graph3Series = value;
                OnPropertyChanged(nameof(Graph3Series));
            }
        }
        public ObservableCollection<IRenderableSeriesViewModel> Graph4Series
        {
            get => graph4Series;
            set
            {
                graph4Series = value;
                OnPropertyChanged(nameof(Graph4Series));
            }
        }
        public ObservableCollection<IRenderableSeriesViewModel> Graph5Series
        {
            get => graph5Series;
            set
            {
                graph5Series = value;
                OnPropertyChanged(nameof(Graph5Series));
            }
        }


        private XyDataSeries<double, double> dataSeriesCap1G1;
        private XyDataSeries<double, double> dataSeriesCap2G1;
        private XyDataSeries<double, double> dataSeriesRocketG1;

        private XyDataSeries<double, double> dataSeriesCap1G2;
        private XyDataSeries<double, double> dataSeriesCap2G2;
        private XyDataSeries<double, double> dataSeriesRocketG2;

        private XyDataSeries<double, double> dataSeriesCap1G3;
        private XyDataSeries<double, double> dataSeriesCap2G3;
        private XyDataSeries<double, double> dataSeriesRocketG3;

        private XyDataSeries<double, double> dataSeriesCap1G4;
        private XyDataSeries<double, double> dataSeriesCap2G4;
        private XyDataSeries<double, double> dataSeriesRocketG4;

        private XyDataSeries<double, double> dataSeriesCap1G5;
        private XyDataSeries<double, double> dataSeriesCap2G5;
        private XyDataSeries<double, double> dataSeriesRocketG5;

        private void InitializeGraph()
        {
            Graph1Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph2Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph3Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph4Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph5Series = new ObservableCollection<IRenderableSeriesViewModel>();


            // Payload
            dataSeriesCap1G1 = new XyDataSeries<double, double>();
            dataSeriesCap2G1 = new XyDataSeries<double, double>();
            dataSeriesRocketG1 = new XyDataSeries<double, double>();
            dataSeriesCap1G1.SeriesName = "Capsule 1";
            dataSeriesCap2G1.SeriesName = "Capsule 2";
            dataSeriesRocketG1.SeriesName = "Rocket";
            dataSeriesCap1G1.AcceptsUnsortedData = true;
            dataSeriesCap2G1.AcceptsUnsortedData = true;
            dataSeriesRocketG1.AcceptsUnsortedData = true;

            dataSeriesCap1G2 = new XyDataSeries<double, double>();
            dataSeriesCap2G2 = new XyDataSeries<double, double>();
            dataSeriesRocketG2 = new XyDataSeries<double, double>();
            dataSeriesCap1G2.SeriesName = "Capsule 1";
            dataSeriesCap2G2.SeriesName = "Capsule 2";
            //dataSeriesRocketG2.SeriesName = "Z-Axis";
            dataSeriesCap1G2.AcceptsUnsortedData = true;
            dataSeriesCap2G2.AcceptsUnsortedData = true;
            dataSeriesRocketG2.AcceptsUnsortedData = true;

            dataSeriesCap1G3 = new XyDataSeries<double, double>();
            dataSeriesCap2G3 = new XyDataSeries<double, double>();
            dataSeriesRocketG3 = new XyDataSeries<double, double>();
            dataSeriesCap1G3.SeriesName = "Capsule 1";
            dataSeriesCap2G3.SeriesName = "Capsule 2";
            //dataSeriesRocketG3.SeriesName = "Z-Axis";
            dataSeriesCap1G3.AcceptsUnsortedData = true;
            dataSeriesCap2G3.AcceptsUnsortedData = true;
            dataSeriesRocketG3.AcceptsUnsortedData = true;

            dataSeriesCap1G4 = new XyDataSeries<double, double>();
            dataSeriesCap2G4 = new XyDataSeries<double, double>();
            dataSeriesRocketG4 = new XyDataSeries<double, double>();
            dataSeriesCap1G4.SeriesName = "Capsule 1";
            dataSeriesCap2G4.SeriesName = "Capsule 2";
            //dataSeriesRocketG4.SeriesName = "Z-Axis";
            dataSeriesCap1G4.AcceptsUnsortedData = true;
            dataSeriesCap2G4.AcceptsUnsortedData = true;
            dataSeriesRocketG4.AcceptsUnsortedData = true;

            dataSeriesCap1G5 = new XyDataSeries<double, double>();
            dataSeriesCap2G5 = new XyDataSeries<double, double>();
            dataSeriesRocketG5 = new XyDataSeries<double, double>();
            dataSeriesCap1G5.SeriesName = "Capsule 1";
            dataSeriesCap2G5.SeriesName = "Capsule 2";
            //dataSeriesRocketG5.SeriesName = "Z-Axis";
            dataSeriesCap1G5.AcceptsUnsortedData = true;
            dataSeriesCap2G5.AcceptsUnsortedData = true;
            dataSeriesRocketG5.AcceptsUnsortedData = true;

            // Chart 1
            Graph1Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G1,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph1Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G1,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            Graph1Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesRocketG1,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 0, 255, 0)
            });

            // Chart 2
            Graph2Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G2,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph2Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G2,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            //Graph2Series.Add(new LineRenderableSeriesViewModel
            //{
            //    DataSeries = dataSeriesRocketG2,
            //    AntiAliasing = false,
            //    StrokeThickness = 1,
            //    ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
            //    Stroke = System.Windows.Media.Color.FromArgb(255, 0, 255, 0)
            //});

            // Chart 3
            Graph3Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G3,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph3Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G3,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            //Graph3Series.Add(new LineRenderableSeriesViewModel
            //{
            //    DataSeries = dataSeriesRocketG3,
            //    AntiAliasing = false,
            //    StrokeThickness = 1,
            //    ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
            //    Stroke = System.Windows.Media.Color.FromArgb(255, 0, 255, 0)
            //});
            // Chart 4
            Graph4Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G4,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph4Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G4,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            //Graph4Series.Add(new LineRenderableSeriesViewModel
            //{
            //    DataSeries = dataSeriesRocketG4,
            //    AntiAliasing = false,
            //    StrokeThickness = 1,
            //    ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
            //    Stroke = System.Windows.Media.Color.FromArgb(255, 0, 255, 0)
            //});
            // Chart 5
            Graph5Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G5,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph5Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G5,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            //Graph5Series.Add(new LineRenderableSeriesViewModel
            //{
            //    DataSeries = dataSeriesRocketG5,
            //    AntiAliasing = false,
            //    StrokeThickness = 1,
            //    ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
            //    Stroke = System.Windows.Media.Color.FromArgb(255, 0, 255, 0)
            //});
            
        }

        #endregion
    }
}
