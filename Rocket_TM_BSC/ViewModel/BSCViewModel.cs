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
using System.Diagnostics;
using GMap.NET.WindowsPresentation;
using GMap.NET;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Threading;


// Welcome to the Bearcat Spaceport Cup Telemetry GUI
// The GUI source is C# written in an MVVM format
// Background Workers are used in model files to create asynch functions that do not affect the UI Thread
// A license to a charting library called SciCharts is required. Students can request educational licenses through their website
// -Carson Billy
namespace Rocket_TM_BSC.ViewModel
{

    public class BSCViewModel : BaseViewModel
    {
        #region Variables

        // Button Commands
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

        public ViewCommand Cap1ReplayCommand { get; set; }
        public ViewCommand Cap2ReplayCommand { get; set; }
        public ViewCommand StartReplayCommand { get; set; }
        public ViewCommand StopReplayCommand { get; set; }
        public ViewCommand RestartReplayCommand { get; set; }

        private string cap1ReplayFile = null;
        private string cap2ReplayFile = null;

        // Class Instances
        private DispatcherTimer _timer;
        private DispatcherTimer _ReplayTimer;
        private Cap1_TM Cap1_Data = new Cap1_TM();
        private Cap2_TM Cap2_Data = new Cap2_TM();
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
        private bool RocketLinkOpen = false;
        private bool Cap1LinkOpen = false;
        private bool Cap2LinkOpen = false;

        #endregion

        // Constructor
        public BSCViewModel()
        {
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

            Cap1ReplayCommand = new ViewCommand(Cap1Replay, CanCap1Replay);
            Cap2ReplayCommand = new ViewCommand(Cap2Replay, CanCap2Replay);
            StopReplayCommand = new ViewCommand(StopReplay, CanStopReplay);
            StartReplayCommand = new ViewCommand(StartReplay, CanStartReplay);
            RestartReplayCommand = new ViewCommand(RestartReplay, CanRestartReplay);

            InitializeGraph();
            
            InitializeGraph_Replay();
            dataSeriesCap1G1.AcceptsUnsortedData = true; // Alt graph
            dataSeriesCap1G2.AcceptsUnsortedData = true; // Velocity Graph
            dataSeriesCap1G3.AcceptsUnsortedData = true; // Temp / Humidity Graph
            dataSeriesCap1G4.AcceptsUnsortedData = true; // VOC Graph
            dataSeriesCap1G5.AcceptsUnsortedData = true; // Satalite Count Graph
            dataSeriesCap1G6.AcceptsUnsortedData = true;
            // Data Replay Graphs
            dataSeriesCap1G7.AcceptsUnsortedData = true;
            dataSeriesCap1G8.AcceptsUnsortedData = true;
            dataSeriesCap1G9.AcceptsUnsortedData = true;
            dataSeriesCap1G10.AcceptsUnsortedData = true;
            dataSeriesCap1G11.AcceptsUnsortedData = true;
            dataSeriesCap1G12.AcceptsUnsortedData = true;
            dataSeriesCap1G13.AcceptsUnsortedData = true;
            dataSeriesCap1G14.AcceptsUnsortedData = true;
            dataSeriesCap1G15.AcceptsUnsortedData = true;

            dataSeriesCap2G1.AcceptsUnsortedData = true; // Alt graph
            dataSeriesCap2G2.AcceptsUnsortedData = true; // Velocity Graph
            dataSeriesCap2G3.AcceptsUnsortedData = true; // Temp / Humidity Graph
            dataSeriesCap2G4.AcceptsUnsortedData = true; // VOC Grap
            dataSeriesCap2G5.AcceptsUnsortedData = true;  // Satalite Count Graph
            dataSeriesCap2G6.AcceptsUnsortedData = true; // Pressure
            // Data Replay Graphs
            dataSeriesCap2G7.AcceptsUnsortedData = true;
            dataSeriesCap2G8.AcceptsUnsortedData = true;
            dataSeriesCap2G9.AcceptsUnsortedData = true;
            dataSeriesCap2G10.AcceptsUnsortedData = true;
            dataSeriesCap2G11.AcceptsUnsortedData = true;
            dataSeriesCap2G12.AcceptsUnsortedData = true;
            dataSeriesCap2G13.AcceptsUnsortedData = true;
            dataSeriesCap2G14.AcceptsUnsortedData = true;
            dataSeriesCap2G15.AcceptsUnsortedData = true;

            dataSeriesRocketG1.AcceptsUnsortedData = true; // Alt graph
            dataSeriesRocketG2.AcceptsUnsortedData = true;
            dataSeriesRocketG3.AcceptsUnsortedData = true;
            dataSeriesRocketG4.AcceptsUnsortedData = true;
            dataSeriesRocketG5.AcceptsUnsortedData = true;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(10);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            //_ReplayTimer = new DispatcherTimer();
            //_ReplayTimer.Interval = TimeSpan.FromMilliseconds(20); // Hz? 20ms = 50hz, 10ms = 100 hz 5ms = 200 hz
            //_ReplayTimer.Tick += _ReplayTimer_Tick; ;
            //_ReplayTimer.Start();

            UpdateComPortCommand.Execute(this);

        }


        #region Timers
        private int i = 1;
        //private int j = 1;
        //Stopwatch stopwatch = new Stopwatch();
        bool flag_C1 = false;
        bool flag_C2 = false;
        private void _timer_Tick(object sender, EventArgs e)
        {
            //stopwatch.Start();
            if (RocketLinkOpen)
            {
                while (Rocket_Data.cap1_DataProcessing_Hex.Cap1_DataOut_Hex.Count > 0)
                {
                    //Console.WriteLine(Rocket_Data.cap1_DataProcessing.Cap1_DataOut.Count);
                    try
                    {
                        // Replace Rocket_Data with Cap1 info
                        Rocket_Data.cap1_DataProcessing_Hex.Cap1_DataOut_Hex.TryDequeue(out var cap1Val);

                        dataSeriesCap1G1.Append(i, cap1Val[10]); // Alt
                        dataSeriesCap1G2.Append(i, cap1Val[8]); // Velo
                        dataSeriesCap1G3.Append(i, cap1Val[12]); // Temp
                        dataSeriesCap1G4.Append(i, cap1Val[11]); // VOC
                        dataSeriesCap1G5.Append(i, cap1Val[3]); // Sat Count

                        Cap1_GPSLat = cap1Val[0].ToString();
                        Cap1_GPSLon = cap1Val[1].ToString();
                        Cap1_SatCount = cap1Val[3].ToString();
                        Cap1_Alt = cap1Val[10].ToString();

                        // Velocity will have to be a seperate thing from accel data
                        i++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            if (Cap1LinkOpen)
            {
                if (flag_C1 == false)
                {
                    flag_C1 = true;
                    Thread.Sleep(50);
                }
                while (Cap1_Data.cap1_DataProcessing_Hex.Cap1_DataOut_Hex.Count > 0)
                {
                    //Console.WriteLine(Rocket_Data.cap1_DataProcessing.Cap1_DataOut.Count);
                    try
                    {
                        // Replace Rocket_Data with Cap1 info
                        Cap1_Data.cap1_DataProcessing_Hex.Cap1_DataOut_Hex.TryDequeue(out var cap1Val);

                        

                        // Velocity will have to be a seperate thing from accel data
                        i++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            
            if (Cap2LinkOpen)
            {
                if (flag_C2 == false)
                {
                    flag_C2 = true;
                    Thread.Sleep(50);
                }
                while (Cap2_Data.cap2_DataProcessing_Hex.Cap2_DataOut_Hex.Count > 0)
                {
                    //Console.WriteLine(Rocket_Data.cap1_DataProcessing.Cap1_DataOut.Count);
                    try
                    {
                        // Replace Rocket_Data with Cap1 info
                        Cap2_Data.cap2_DataProcessing_Hex.Cap2_DataOut_Hex.TryDequeue(out var cap2Val);

                        dataSeriesCap2G1.Append(i, cap2Val[11]); // Alt
                        dataSeriesCap2G2.Append(i, cap2Val[8]); // Velo
                        dataSeriesCap2G3.Append(i, cap2Val[12]); // Temp
                        dataSeriesCap2G5.Append(i, cap2Val[3]); // Sat Count

                        Cap2_GPSLat = cap2Val[0].ToString();
                        Cap2_GPSLon = cap2Val[1].ToString();
                        Cap2_SatCount = cap2Val[3].ToString();
                        Cap2_Alt = cap2Val[10].ToString();
                        // Velocity will have to be a seperate thing from accel data
                        i++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            //Console.WriteLine(stopwatch.ElapsedMilliseconds + ": " + j);
            //j++;
            //stopwatch.Reset();

        }

        private int replayCount = 0;
        private bool ReplayStart = false;
        private bool ReplayStop = true;
        private bool ReplayRestart = false;
        private int Countmax = 1;
        private void _ReplayTimer_Tick(object sender, EventArgs e)
        {
            if (!ReplayStop)
            {
                if (ReplayRestart)
                {
                    replayCount = Countmax; // Sends to Restart Loop
                    ReplayRestart = false;
                }
                if (replayCount < Countmax && ReplayStart)
                {
                    // Add data to plots. Set timer tick to represent sample rate
                    replayCount++;
                }
                if (replayCount >= 1)
                {
                    replayCount = 0;
                    // Clear Graphs
                    // Restarts the data replay
                }
            }
           
            
            // Use concurrent queue or use indivud
            // Add all graphing and data display for data replay tab
        }

        #endregion

        #region Public Bindings
        // Sets all data bindings linking the mainwindow.xaml to the view model

        public string Cap1ReplayFile
        {
            get { return cap1ReplayFile; }
            set { cap1ReplayFile = value; OnPropertyChanged("Cap1ReplayFile"); }
        }
        public string Cap2ReplayFile
        {
            get { return cap2ReplayFile; }
            set { cap2ReplayFile = value; OnPropertyChanged("Cap2ReplayFile"); }
        }
        

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
        // Contains methods for each button command

        // Updates COM port list
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

        // Opens the rocket TM COM port
        public void OpenRocketCOM(object obj)
        {
            //Console.WriteLine(RocketCOM);
            Rocket_Data.OpenNewPort(RocketCOM);
            RocketLinkOpen = true;
            WakeRocket.RaiseCanExecuteChanged();
            StatusCheckRocket.RaiseCanExecuteChanged();
            
        }

        public bool CanOpenRocketCOM(object obj)
        {
            return true;
        }

        // Opens capsule 1's COM port
        public void OpenCap1COM(object obj)
        {
            //Console.WriteLine(RocketCOM);
            Cap1_Data.OpenNewPort(Cap1COM);
            Cap1LinkOpen = true;
            WakeCap1.RaiseCanExecuteChanged();
            StatusCheckCap1.RaiseCanExecuteChanged();
        }

        public bool CanOpenCap1COM(object obj)
        {
            return true;
        }

        // Opens capsule 2's COM port
        public void OpenCap2COM(object obj)
        {
            //Console.WriteLine(RocketCOM);
            Cap2_Data.OpenNewPort(Cap2COM);
            Cap2LinkOpen = true;
            WakeCap2.RaiseCanExecuteChanged();
            StatusCheckCap2.RaiseCanExecuteChanged();

        }

        public bool CanOpenCap2COM(object obj)
        {
            return true;
        }

        // Sends Status Check command to receive one data frame from rocket TM
        public void StatCheckRocket(object obj)
        {
            // Sends Status Check command to TM to update view
            Rocket_Data.CommandStringTM1.Enqueue("transmit_data");

        }

        public bool CanStatCheckRocket(object obj)
        {
            return RocketLinkOpen;
        }

        // Sends Status Check command to receive one data frame from capsule 1 TM
        public void StatCheckCap1(object obj)
        {
            // Sends Status Check command to TM to update view
            Cap1_Data.CommandStringTM1.Enqueue("transmit_data");
        }

        public bool CanStatCheckCap1(object obj)
        {
            return Cap1LinkOpen;
        }

        // Sends Status Check command to receive one data frame from capsule 2 TM
        public void StatCheckCap2(object obj)
        {
            // Sends Status Check command to TM to update view
            Cap2_Data.CommandStringTM2.Enqueue("transmit_data");
            
        }

        public bool CanStatCheckCap2(object obj)
        {
            return Cap2LinkOpen;
        }

        // Sends wake command to activate full TM link and hardware loop for launch
        public void WakeUpRocket(object obj)
        {
            // Sends Wake Command to TM to activate for launch
            Rocket_Data.CommandStringTM1.Enqueue("WAKE");
        }

        public bool CanWakeUpRocket(object obj)
        {
            return RocketLinkOpen;
        }

        // Sends wake command to activate full TM link and hardware loop for launch
        public void WakeUpCap1(object obj)
        {
            // Sends Wake Command to TM to activate for launch
            Cap1_Data.CommandStringTM1.Enqueue("start");
        }

        public bool CanWakeUpCap1(object obj)
        {
            return Cap1LinkOpen;
        }

        // Sends wake command to activate full TM link and hardware loop for launch
        public void WakeUpCap2(object obj)
        {
            // Sends Wake Command to TM to activate for launch
            Cap2_Data.CommandStringTM2.Enqueue("WAKE");
        }

        public bool CanWakeUpCap2(object obj)
        {
            return Cap2LinkOpen;
        }

        private OpenFileDialog ofd;
        private OpenFileDialog ofd2;
        public void Cap1Replay(object obj)
        {
            ofd = new OpenFileDialog();
            ofd.Filter = "csv files (*.csv)|*.csv";
            ofd.ShowDialog();
            string cap1ReplayPath = ofd.FileName;
            Cap1ReplayFile = cap1ReplayPath;
        }

        public bool CanCap1Replay(object obj)
        {
            return true;
        }


        public void Cap2Replay(object obj)
        {
            ofd2 = new OpenFileDialog();
            ofd2.Filter = "csv files (*.csv)|*.csv";
            ofd2.ShowDialog();
            string cap2ReplayPath = ofd2.FileName;
            Cap2ReplayFile = cap2ReplayPath;
        }

        
        public bool CanCap2Replay(object obj)
        {
            return true;
        }

        private DataReplay_Cap1 _cap1Replay;
        private DataReplay_Cap1 _cap2Replay;
        public void StartReplay(object obj)
        {
            _cap1Replay = new DataReplay_Cap1();
            _cap1Replay.RunDataPlayback(Cap1ReplayFile,Cap2ReplayFile);
        }

        public bool CanStartReplay(object obj)
        {
            return true;
        }

        public void StopReplay(object obj)
        {
            ReplayStop = true;
        }

        public bool CanStopReplay(object obj)
        {
            return true;
        }

        public void RestartReplay(object obj)
        {
            ReplayRestart=true;
        }

        public bool CanRestartReplay(object obj)
        {
            return true;
        }
        #endregion

        // Mapping
        #region Mapping Functions
        // Need to figure out MVVM for GMaps
        private void AddLatLon(double Lat, double Lon, string Cap_Num)
        {
            GMapMarker marker = new GMapMarker(new PointLatLng(39.86113302187091, -83.6557333146190));
            marker.Shape = new Image
            {
                Width = 25,
                Height = 25,
                Source = new BitmapImage(new System.Uri("pack://application:,,,/assets/MarkerIcon.png"))
            };
        }
        
        
        #endregion


        #region SciChartTests
        public static Stream StringToStream(string src)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(src);
            return new MemoryStream(byteArray);
        }
        #endregion

        // SciCharts graphing
        #region 2D Graphing
        
        private ObservableCollection<IRenderableSeriesViewModel> graph1Series { get; set; }
        private ObservableCollection<IRenderableSeriesViewModel> graph2Series { get; set; }
        private ObservableCollection<IRenderableSeriesViewModel> graph3Series { get; set; }
        private ObservableCollection<IRenderableSeriesViewModel> graph4Series { get; set; }
        private ObservableCollection<IRenderableSeriesViewModel> graph5Series { get; set; }
        private ObservableCollection<IRenderableSeriesViewModel> graph6Series { get; set; }
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

        public ObservableCollection<IRenderableSeriesViewModel> Graph6Series
        {
            get => graph6Series;
            set
            {
                graph6Series = value;
                OnPropertyChanged(nameof(Graph6Series));
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

        private XyDataSeries<double, double> dataSeriesCap1G6;
        private XyDataSeries<double, double> dataSeriesCap2G6;
        

        private void InitializeGraph()
        {
            Graph1Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph2Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph3Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph4Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph5Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph6Series = new ObservableCollection<IRenderableSeriesViewModel>();


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

            dataSeriesCap1G6 = new XyDataSeries<double, double>();
            dataSeriesCap2G6 = new XyDataSeries<double, double>();
            
            dataSeriesCap1G6.SeriesName = "Capsule 1";
            dataSeriesCap2G6.SeriesName = "Capsule 2";
            
            dataSeriesCap1G6.AcceptsUnsortedData = true;
            dataSeriesCap2G6.AcceptsUnsortedData = true;
            

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

            
            // Graph 6
            Graph6Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G6,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph6Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G6,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

        }

        #endregion

        #region Data Replay Graphs

        private ObservableCollection<IRenderableSeriesViewModel> graph7Series { get; set; }
        public ObservableCollection<IRenderableSeriesViewModel> Graph7Series
        {
            get => graph7Series;
            set
            {
                graph7Series = value;
                OnPropertyChanged(nameof(Graph7Series));
            }
        }

        private ObservableCollection<IRenderableSeriesViewModel> graph8Series { get; set; }
        public ObservableCollection<IRenderableSeriesViewModel> Graph8Series
        {
            get => graph8Series;
            set
            {
                graph8Series = value;
                OnPropertyChanged(nameof(Graph8Series));
            }
        }

        private ObservableCollection<IRenderableSeriesViewModel> graph9Series { get; set; }
        public ObservableCollection<IRenderableSeriesViewModel> Graph9Series
        {
            get => graph9Series;
            set
            {
                graph9Series = value;
                OnPropertyChanged(nameof(Graph9Series));
            }
        }

        private ObservableCollection<IRenderableSeriesViewModel> graph10Series { get; set; }
        public ObservableCollection<IRenderableSeriesViewModel> Graph10Series
        {
            get => graph10Series;
            set
            {
                graph10Series = value;
                OnPropertyChanged(nameof(Graph10Series));
            }
        }

        private ObservableCollection<IRenderableSeriesViewModel> graph11Series { get; set; }
        public ObservableCollection<IRenderableSeriesViewModel> Graph11Series
        {
            get => graph11Series;
            set
            {
                graph11Series = value;
                OnPropertyChanged(nameof(Graph11Series));
            }
        }

        private ObservableCollection<IRenderableSeriesViewModel> graph12Series { get; set; }
        public ObservableCollection<IRenderableSeriesViewModel> Graph12Series
        {
            get => graph12Series;
            set
            {
                graph12Series = value;
                OnPropertyChanged(nameof(Graph12Series));
            }
        }

        private ObservableCollection<IRenderableSeriesViewModel> graph13Series { get; set; }
        public ObservableCollection<IRenderableSeriesViewModel> Graph13Series
        {
            get => graph3Series;
            set
            {
                graph13Series = value;
                OnPropertyChanged(nameof(Graph3Series));
            }
        }

        private ObservableCollection<IRenderableSeriesViewModel> graph14Series { get; set; }
        public ObservableCollection<IRenderableSeriesViewModel> Graph14Series
        {
            get => graph14Series;
            set
            {
                graph14Series = value;
                OnPropertyChanged(nameof(Graph14Series));
            }
        }

        private ObservableCollection<IRenderableSeriesViewModel> graph15Series { get; set; }
        public ObservableCollection<IRenderableSeriesViewModel> Graph15Series
        {
            get => graph15Series;
            set
            {
                graph15Series = value;
                OnPropertyChanged(nameof(Graph15Series));
            }
        }

        private XyDataSeries<double, double> dataSeriesCap1G7;
        private XyDataSeries<double, double> dataSeriesCap2G7;

        private XyDataSeries<double, double> dataSeriesCap1G8;
        private XyDataSeries<double, double> dataSeriesCap2G8;

        private XyDataSeries<double, double> dataSeriesCap1G9;
        private XyDataSeries<double, double> dataSeriesCap2G9;

        private XyDataSeries<double, double> dataSeriesCap1G10;
        private XyDataSeries<double, double> dataSeriesCap2G10;

        private XyDataSeries<double, double> dataSeriesCap1G11;
        private XyDataSeries<double, double> dataSeriesCap2G11;

        private XyDataSeries<double, double> dataSeriesCap1G12;
        private XyDataSeries<double, double> dataSeriesCap2G12;

        private XyDataSeries<double, double> dataSeriesCap1G13;
        private XyDataSeries<double, double> dataSeriesCap2G13;

        private XyDataSeries<double, double> dataSeriesCap1G14;
        private XyDataSeries<double, double> dataSeriesCap2G14;

        private XyDataSeries<double, double> dataSeriesCap1G15;
        private XyDataSeries<double, double> dataSeriesCap2G15;



        private void InitializeGraph_Replay()
        {
            Graph7Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph8Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph9Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph10Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph11Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph12Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph13Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph14Series = new ObservableCollection<IRenderableSeriesViewModel>();
            Graph15Series = new ObservableCollection<IRenderableSeriesViewModel>();


            // Payload
            dataSeriesCap1G7 = new XyDataSeries<double, double>();
            dataSeriesCap2G7 = new XyDataSeries<double, double>();
            dataSeriesCap1G7.SeriesName = "Capsule 1";
            dataSeriesCap2G7.SeriesName = "Capsule 2";
            dataSeriesCap1G7.AcceptsUnsortedData = true;
            dataSeriesCap2G7.AcceptsUnsortedData = true;

            dataSeriesCap1G8 = new XyDataSeries<double, double>();
            dataSeriesCap2G8 = new XyDataSeries<double, double>();
            dataSeriesCap1G8.SeriesName = "Capsule 1";
            dataSeriesCap2G8.SeriesName = "Capsule 2";
            dataSeriesCap1G8.AcceptsUnsortedData = true;
            dataSeriesCap2G8.AcceptsUnsortedData = true;

            dataSeriesCap1G9 = new XyDataSeries<double, double>();
            dataSeriesCap2G9 = new XyDataSeries<double, double>();
            dataSeriesCap1G9.SeriesName = "Capsule 1";
            dataSeriesCap2G9.SeriesName = "Capsule 2";
            dataSeriesCap1G9.AcceptsUnsortedData = true;
            dataSeriesCap2G9.AcceptsUnsortedData = true;

            dataSeriesCap1G10 = new XyDataSeries<double, double>();
            dataSeriesCap2G10 = new XyDataSeries<double, double>();
            dataSeriesCap1G10.SeriesName = "Capsule 1";
            dataSeriesCap2G10.SeriesName = "Capsule 2";
            dataSeriesCap1G10.AcceptsUnsortedData = true;
            dataSeriesCap2G10.AcceptsUnsortedData = true;

            dataSeriesCap1G11 = new XyDataSeries<double, double>();
            dataSeriesCap2G11 = new XyDataSeries<double, double>();
            dataSeriesCap1G11.SeriesName = "Capsule 1";
            dataSeriesCap2G11.SeriesName = "Capsule 2";
            dataSeriesCap1G11.AcceptsUnsortedData = true;
            dataSeriesCap2G11.AcceptsUnsortedData = true;

            dataSeriesCap1G12 = new XyDataSeries<double, double>();
            dataSeriesCap2G12 = new XyDataSeries<double, double>();
            dataSeriesCap1G12.SeriesName = "Capsule 1";
            dataSeriesCap2G12.SeriesName = "Capsule 2";
            dataSeriesCap1G12.AcceptsUnsortedData = true;
            dataSeriesCap2G12.AcceptsUnsortedData = true;

            dataSeriesCap1G13 = new XyDataSeries<double, double>();
            dataSeriesCap2G13 = new XyDataSeries<double, double>();
            dataSeriesCap1G13.SeriesName = "Capsule 1";
            dataSeriesCap2G13.SeriesName = "Capsule 2";
            dataSeriesCap1G13.AcceptsUnsortedData = true;
            dataSeriesCap2G13.AcceptsUnsortedData = true;

            dataSeriesCap1G14 = new XyDataSeries<double, double>();
            dataSeriesCap2G14 = new XyDataSeries<double, double>();
            dataSeriesCap1G14.SeriesName = "Capsule 1";
            dataSeriesCap2G14.SeriesName = "Capsule 2";
            dataSeriesCap1G14.AcceptsUnsortedData = true;
            dataSeriesCap2G14.AcceptsUnsortedData = true;

            dataSeriesCap1G15 = new XyDataSeries<double, double>();
            dataSeriesCap2G15 = new XyDataSeries<double, double>();
            dataSeriesCap1G15.SeriesName = "Capsule 1";
            dataSeriesCap2G15.SeriesName = "Capsule 2";
            dataSeriesCap1G15.AcceptsUnsortedData = true;
            dataSeriesCap2G15.AcceptsUnsortedData = true;


            // Chart 1
            Graph7Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G7,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph7Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G7,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            // Chart 2
            Graph8Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G8,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph8Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G8,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            

            // Chart 3
            Graph9Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G9,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph9Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G9,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });


            // Chart 4
            Graph10Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G10,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph10Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G10,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            
            // Chart 5
            Graph11Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G11,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph11Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G11,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            
            // Chart 5
            Graph12Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G12,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph12Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G12,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            // Chart 5
            Graph13Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G13,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph13Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G13,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            // Chart 5
            Graph14Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G14,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph14Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G14,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            // Chart 5
            Graph15Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G15,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph15Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G15,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

        }
        #endregion
    }
}
