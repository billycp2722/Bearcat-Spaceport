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
using SciChart.Data.Model;
using SimpleMvvmToolkit;
using System.Reflection;


// Welcome to the Bearcat Spaceport Cup Telemetry GUI
// The GUI source is C# written in an MVVM format
// Background Workers are used in model files to create asynch functions that do not affect the UI Thread
// A license to a charting library called SciCharts is required. Students can request educational licenses through their website
// -Carson Billy
namespace Rocket_TM_BSC.ViewModel
{
    // TODO:
    // 1. Configure RSSI In
    // 2. Configure Payload TM Data
    // 3. Add Accel to Velo Function?
    // 4. Choose Main TM View Graphs
    // 5. Add Time scaling / change samples to time or time to samples
    // 6. Add Statistics to data replay
    // 7. Retest TM changes with live TM link
    // 8. Check Final Replay version functions, Causing issues with UI thread
    // // Radio Setup: RSSI 500 ms, ROCK ID 1015 COM 4
    // CAM: ID 1090 COM 9
    // ATM: COM 8
    // ARduino: COM 6
    public class BSCViewModel : BaseViewModel
    {
        #region Variables

        // Button Commands
        public ViewCommand UpdateComPortCommand { get; set; }
        public ViewCommand OpenRocketCOMCommand { get; set; }
        public ViewCommand OpenCap1COMCommand { get; set; }
        public ViewCommand OpenCap2COMCommand { get; set; }
        public ViewCommand OpenRSSICOMCommand { get; set; }
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
        public ViewCommand EjectRocket { get; set; }

        public ViewCommand FillPV { get; set; }

        private string cap1ReplayFile = null;
        private string cap2ReplayFile = null;

        // Class Instances
        private DispatcherTimer _timer;
        private DispatcherTimer _ReplayTimer;
        private Cap1_TM Cap1_Data = new Cap1_TM();
        private Cap2_TM Cap2_Data = new Cap2_TM();
        private Rocket_TM Rocket_Data = new Rocket_TM();
        

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
        private string rocketAlt = "0";
        private string apogeeAlt = "0";
        private string sysPressure1 = "0";
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
        public event EventHandler<NotificationEventArgs<string>> DoSomething;
        public event EventHandler<NotificationEventArgs<string>> DoSomething2;
        public event EventHandler<NotificationEventArgs<string>> Cap1TrackEvent;
        public event EventHandler<NotificationEventArgs<string>> Cap2TrackEvent;
        public BSCViewModel()
        {
            // Configure View Commands
            UpdateComPortCommand = new ViewCommand(UpdateComPort, CanUpdateComPort);
            OpenRocketCOMCommand = new ViewCommand(OpenRocketCOM, CanOpenRocketCOM);
            OpenCap1COMCommand = new ViewCommand(OpenCap1COM, CanOpenCap1COM);
            OpenCap2COMCommand = new ViewCommand(OpenCap2COM, CanOpenCap2COM);
            OpenRSSICOMCommand = new ViewCommand(OpenRSSICOM, CanOpenRSSICOM);

            StatusCheckRocket = new ViewCommand(StatCheckRocket, CanStatCheckRocket);
            StatusCheckCap1 = new ViewCommand(StatCheckCap1, CanStatCheckCap1);
            StatusCheckCap2 = new ViewCommand(StatCheckCap2, CanStatCheckCap2);

            WakeRocket = new ViewCommand(WakeUpRocket, CanWakeUpRocket);
            WakeCap1 = new ViewCommand(WakeUpCap1, CanWakeUpCap1);
            WakeCap2 = new ViewCommand(WakeUpCap2, CanWakeUpCap2);
            //
            Cap1ReplayCommand = new ViewCommand(Cap1Replay, CanCap1Replay);
            Cap2ReplayCommand = new ViewCommand(Cap2Replay, CanCap2Replay);
            StopReplayCommand = new ViewCommand(StopReplay, CanStopReplay);
            StartReplayCommand = new ViewCommand(StartReplay, CanStartReplay);
            RestartReplayCommand = new ViewCommand(RestartReplay, CanRestartReplay);
            EjectRocket = new ViewCommand(EjectPayload, CanEjectPayload);
            FillPV = new ViewCommand(FillPressure, CanFillPressure);

            // Initalize Graphs
            InitializeGraph(); 
            InitializeGraph_Replay();

            // Initalize Timers
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(15);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _ReplayTimer = new DispatcherTimer();
            _ReplayTimer.Interval = TimeSpan.FromMilliseconds(12); // Hz? 20ms = 50hz, 10ms = 100 hz 5ms = 200 hz
            _ReplayTimer.Tick += _ReplayTimer_Tick; ;
            _ReplayTimer.Start();

            UpdateComPortCommand.Execute(this);
            TimeRangeG2 = new DoubleRange(0, 800);
            TimeRangeG4 = new DoubleRange(0, 800);     
            TimeRangeG5 = new DoubleRange(0, 800);
            TimeRangeG6 = new DoubleRange(0, 800);

        }


        #region Timers
        private int i = 1;
        //private int j = 1;
        //Stopwatch stopwatch = new Stopwatch();
        bool flag_C1 = false;
        bool flag_C2 = false;
        bool flag_R = false;
        int mapPlot = 100;
        int mapPlot2 = 100;
        int plotC1 = 100;
        int plotC2 = 100;


        int GraphRangeCounter = 0;
        int GraphRangeCounter2 = 0;
        bool cap1Flag = false;
        double apogeeMax = 0;
        
        private void _timer_Tick(object sender, EventArgs e)
        {
            int tempInc = i;
            //stopwatch.Start();
            if (RocketLinkOpen)
            {
                if (flag_R == false)
                {
                    flag_R = true;
                    Thread.Sleep(50);
                }
                while (Rocket_Data.Rocket_DataProcessing_Hex.Rocket_DataOut_Hex.Count > 0)
                {
                    RockTMStat = Brushes.Green;
                    if (flag_R == false)
                    {
                        flag_R = true;
                        Thread.Sleep(50);
                    }
                    //Console.WriteLine(Rocket_Data.cap1_DataProcessing.Cap1_DataOut.Count);
                    try
                    {
                        // Replace Rocket_Data with Cap1 info
                        Rocket_Data.Rocket_DataProcessing_Hex.Rocket_DataOut_Hex.TryDequeue(out var cap1Val);

                        dataSeriesRocketG1.Append(i, cap1Val[0]); // Alt
                        dataSeriesCap1G6.Append(i, cap1Val[1]); // Pressure
                        SysPressure1 = cap1Val[1].ToString();
                        RocketAlt = cap1Val[0].ToString();
                        RocketSigStrength = Rocket_Data.FrameRate3.ToString();
                        if (cap1Val[0] > apogeeMax)
                        {
                            apogeeMax = cap1Val[0];
                            ApogeeAlt = apogeeMax.ToString();
                        }

                        if (GraphRangeCounter2 > 200 && i > 800)
                        {
                            TimeRangeG6 = new DoubleRange(i - 800, i + 200);
                            GraphRangeCounter2 = 0;
                        }
                        GraphRangeCounter2++;

                        if (cap1Val[2] == 0)
                        {
                            CapEject = Brushes.Green;
                        }
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
                    Cap1TMStat = Brushes.Green;
                    //Console.WriteLine(Rocket_Data.cap1_DataProcessing.Cap1_DataOut.Count);
                    try
                    {
                        // Replace Rocket_Data with Cap1 info
                        Cap1_Data.cap1_DataProcessing_Hex.Cap1_DataOut_Hex.TryDequeue(out var cap1Val);
                       
                        double AccelMag = Math.Sqrt(Math.Pow(cap1Val[7],2)+ Math.Pow(cap1Val[8], 2)+ Math.Pow(cap1Val[9],2));
                        string s = string.Format("{0:N4}", AccelMag);
                        dataSeriesCap1G1.Append(i, cap1Val[10]); // Alt
                        dataSeriesCap1G2X.Append(i, cap1Val[7]); // Accel
                        dataSeriesCap1G2Y.Append(i, cap1Val[8]); // Accel
                        dataSeriesCap1G2Z.Append(i, cap1Val[9]); // Accel
                        dataSeriesCap1G3.Append(i, cap1Val[12]); // Temp
                        dataSeriesCap1G5.Append(i, cap1Val[3]); // Sat Count
                        //dataSeriesCap1G4.Append(i, cap1Val[11]); // VOC

                        Cap1_GPSLat = cap1Val[0].ToString();
                        Cap1_GPSLon = cap1Val[1].ToString();
                        Cap1_SatCount = cap1Val[3].ToString();
                        Cap1_Alt = cap1Val[10].ToString();
                        Cap1_Velo = s;

                        LostFrame_Cap1 = Cap1_Data.lost_frames.ToString();
                        DataRate_Cap1 = Cap1_Data.FrameRate.ToString();
                        // Velocity will have to be a seperate thing from accel data
                        if (plotC1 >= 100)
                        {
                            AddLatLonCap1(cap1Val[0], cap1Val[1]);
                            plotC1 = 0;
                        }
                        if (GraphRangeCounter > 200 && i>800)
                        {
                            TimeRangeG2 = new DoubleRange(i - 800, i + 200);
                            TimeRangeG5 = TimeRangeG2;
                            GraphRangeCounter = 0;
                        }
                        cap1Flag = true;
                        GraphRangeCounter++;
                        plotC1++;
                        
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
                    Cap2TMStat = Brushes.Green;
                    //Console.WriteLine(Rocket_Data.cap1_DataProcessing.Cap1_DataOut.Count);
                    try
                    {
                        // Replace Rocket_Data with Cap1 info
                        Cap2_Data.cap2_DataProcessing_Hex.Cap2_DataOut_Hex.TryDequeue(out var cap2Val);

                        double AccelMag = Math.Sqrt(Math.Pow(cap2Val[7], 2) + Math.Pow(cap2Val[8], 2) + Math.Pow(cap2Val[9], 2));
                        string s = string.Format("{0:N4}", AccelMag);
                        dataSeriesCap2G1.Append(i, cap2Val[10]); // Alt
                        dataSeriesCap2G3X.Append(i, cap2Val[7]); // Velo
                        dataSeriesCap2G3Y.Append(i, cap2Val[8]);
                        dataSeriesCap2G3Z.Append(i, cap2Val[9]);
                        dataSeriesCap2G5.Append(i, cap2Val[3]); // Sat Count

                        Cap2_GPSLat = cap2Val[0].ToString();
                        Cap2_GPSLon = cap2Val[1].ToString();
                        Cap2_SatCount = cap2Val[3].ToString();
                        Cap2_Alt = cap2Val[10].ToString();
                        Cap2_Velo = s;
                        // Velocity will have to be a seperate thing from accel data
                        if (plotC2 >= 100)
                        {
                            AddLatLonCap2(cap2Val[0], cap2Val[1]);
                            plotC2 = 0;
                        }
                        LostFrame_Cap2 = Cap2_Data.lost_frames2.ToString();
                        DataRate_Cap2 = Cap2_Data.FrameRate2.ToString();

                        if (GraphRangeCounter > 200 && i > 800)
                        {
                            TimeRangeG4 = new DoubleRange(i - 800, i + 200);
                            TimeRangeG5 = new DoubleRange(i - 800, i + 200);
                            GraphRangeCounter = 0;
                        }
                        if (cap1Flag == false)
                        {
                            GraphRangeCounter++;
                        } 
                        plotC2++;
                       
                        i++;
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }

            Cap1Strength = RSSI.RSSI_Cap1.ToString();
            if (RSSI.RSSI_Cap1 != 0)
            {
                if (RSSI.RSSI_Cap1 < -95)
                {
                    Cap1TMStat = Brushes.Yellow;
                }
                if (RSSI.RSSI_Cap1 < -105)
                {
                    Cap1TMStat = Brushes.Red;
                }
                if (RSSI.RSSI_Cap1 < 0)
                {
                    Cap1TMStat = Brushes.Green;
                }
            }

            Cap2Strength = RSSI.RSSI_Cap2.ToString();
            if (RSSI.RSSI_Cap2 != 0)
            {
                if (RSSI.RSSI_Cap2 < -95)
                {
                    Cap2TMStat = Brushes.Yellow;
                }
                if (RSSI.RSSI_Cap2 < -105)
                {
                    Cap2TMStat = Brushes.Red;
                }
                if (RSSI.RSSI_Cap2 < 0)
                {
                    Cap2TMStat = Brushes.Green;
                }
                
            }

            RocketSigStrength = RSSI.RSSI_Rocket.ToString();
            if (RSSI.RSSI_Rocket != 0)
            {
                if(RSSI.RSSI_Rocket < -95)
                {
                    RockTMStat = Brushes.Yellow;
                }
                if (RSSI.RSSI_Rocket < -105)
                {
                    RockTMStat = Brushes.Red;
                }
                if (RSSI.RSSI_Rocket < 0)
                {
                    RockTMStat = Brushes.Green;
                }
            }
            //Console.WriteLine(stopwatch.ElapsedMilliseconds + ": " + j);
            //j++;
            //stopwatch.Reset();

        }

        private int replayCount = 0;
        private int replayCount2 = 0;
        private bool ReplayStart = false;
        private bool ReplayStop = true;
        private bool ReplayRestart = false;
        private int Countmax = 1;
        private int Countmax2 = 1;
        private Stopwatch replaySW = new Stopwatch();
        private int UpdateRange = 100;
        private int UpdateRange2 = 100;
        private double maxaccel = 0;
        private void _ReplayTimer_Tick(object sender, EventArgs e)
        {
            if (!ReplayStop)
            {
                if (_cap1Replay.Replay_Cap1_Ready)
                {
                    Countmax = _cap1Replay.DP1.Length;
                    
                    if (ReplayRestart)
                    {
                        replayCount = Countmax; // Sends to Restart Loop
                        ReplayRestart = false;
                    }
                    if (replayCount < Countmax && ReplayStart)
                    {
                        
                        replaySW.Start();
                        if (replayCount > 1000 && Countmax >= Countmax2 && UpdateRange >= 100)
                        {
                            TimeRangeG7 = new DoubleRange(replayCount-899, replayCount + 200);
                            TimeRangeG8 = new DoubleRange(replayCount - 899, replayCount + 200);
                            TimeRangeG9 = new DoubleRange(replayCount - 899, replayCount + 200);
                            TimeRangeG10 = new DoubleRange(replayCount - 899, replayCount + 200);
                            TimeRangeG11 = new DoubleRange(replayCount - 899, replayCount + 200);
                            TimeRangeG12 = new DoubleRange(replayCount - 899, replayCount + 200);
                            TimeRangeG13 = new DoubleRange(replayCount - 899, replayCount + 200);
                            TimeRangeG14 = new DoubleRange(replayCount - 899, replayCount + 200);
                            TimeRangeG15 = new DoubleRange(replayCount - 899, replayCount + 200);
                            TimeRangeG16 = new DoubleRange(replayCount - 899, replayCount + 200);
                            UpdateRange = 0;
                        }
                        else if(UpdateRange >= 100)
                        {
                            TimeRangeG7 = new DoubleRange(0, replayCount+200);
                            TimeRangeG8 = new DoubleRange(0, replayCount + 200);
                            TimeRangeG9 = new DoubleRange(0, replayCount + 200);
                            TimeRangeG10 = new DoubleRange(0, replayCount + 200);
                            TimeRangeG11 = new DoubleRange(0, replayCount + 200);
                            TimeRangeG12 = new DoubleRange(0, replayCount + 200);
                            TimeRangeG13 = new DoubleRange(0, replayCount + 200);
                            TimeRangeG14 = new DoubleRange(0, replayCount + 200);
                            TimeRangeG15 = new DoubleRange(0, replayCount + 200);
                            TimeRangeG16 = new DoubleRange(0, replayCount + 200);
                            UpdateRange = 0;
                        }
                        UpdateRange++;
                        
                        // Add data to plots. Set timer tick to represent sample rate
                        dataSeriesCap1G7.Append(replayCount, _cap1Replay.DP9[replayCount]); // Alt
                        dataSeriesCap1G16.Append(replayCount, _cap1Replay.DP3[replayCount]); // GPS Alt
                        dataSeriesCap1G8.Append(replayCount, _cap1Replay.DP4[replayCount]); // Sat Count
                        dataSeriesCap1G9X.Append(replayCount, _cap1Replay.DP6[replayCount]); // Accel X
                        dataSeriesCap1G9Y.Append(replayCount, _cap1Replay.DP7[replayCount]); // Accel Y
                        dataSeriesCap1G9Z.Append(replayCount, _cap1Replay.DP8[replayCount]); // Accel Z
                        //dataSeriesCap1G10.Append(replayCount, _cap1Replay.DP10[replayCount]); // VOC
                        //dataSeriesCap1G11.Append(replayCount, _cap1Replay.DP11[replayCount]); // Humid
                        //dataSeriesCap1G12.Append(replayCount, _cap1Replay.DP12[replayCount]); // Temp
                        dataSeriesCap1XG13.Append(replayCount, _cap1Replay.DP10[replayCount]); // gyro X
                        dataSeriesCap1YG13.Append(replayCount, _cap1Replay.DP14[replayCount]); // gyro X
                        dataSeriesCap1ZG13.Append(replayCount, _cap1Replay.DP15[replayCount]); // gyro X
                        double AccelMag = Math.Sqrt(Math.Pow(_cap1Replay.DP6[replayCount], 2) + Math.Pow(_cap1Replay.DP7[replayCount], 2) + Math.Pow(_cap1Replay.DP8[replayCount], 2));
                        dataSeriesCap1G15.Append(replayCount, AccelMag); // Accel X Cap2
                        if (maxaccel < AccelMag)
                        {
                            DP_2 = AccelMag;
                            maxaccel = AccelMag;
                        }
                        replayCount++;
                        if (replayCount < Countmax)
                        {
                            dataSeriesCap1G7.Append(replayCount, _cap1Replay.DP9[replayCount]); // Alt
                            dataSeriesCap1G16.Append(replayCount, _cap1Replay.DP3[replayCount]); // GPS Alt
                            dataSeriesCap1G8.Append(replayCount, _cap1Replay.DP4[replayCount]); // Sat Count
                            dataSeriesCap1G9X.Append(replayCount, _cap1Replay.DP6[replayCount]); // Accel X
                            dataSeriesCap1G9Y.Append(replayCount, _cap1Replay.DP7[replayCount]); // Accel Y
                            dataSeriesCap1G9Z.Append(replayCount, _cap1Replay.DP8[replayCount]); // Accel Z
                            //dataSeriesCap1G10.Append(replayCount, _cap1Replay.DP10[replayCount]); // VOC
                            //dataSeriesCap1G11.Append(replayCount, _cap1Replay.DP11[replayCount]); // Humid
                            //dataSeriesCap1G12.Append(replayCount, _cap1Replay.DP12[replayCount]); // Temp
                            dataSeriesCap1XG13.Append(replayCount, _cap1Replay.DP10[replayCount]); // gyro X
                            dataSeriesCap1YG13.Append(replayCount, _cap1Replay.DP14[replayCount]);
                            dataSeriesCap1ZG13.Append(replayCount, _cap1Replay.DP15[replayCount]);
                            AccelMag = Math.Sqrt(Math.Pow(_cap1Replay.DP6[replayCount],2) + Math.Pow(_cap1Replay.DP7[replayCount],2) + Math.Pow(_cap1Replay.DP8[replayCount],2));
                            dataSeriesCap1G15.Append(replayCount, AccelMag); // Accel X Cap2
                        }
                        
                        if (maxaccel < AccelMag)
                        {
                            DP_2 = AccelMag;
                            maxaccel = AccelMag;
                        }
                        if (mapPlot >= 10)
                        {
                            AddLatLonReplay(_cap1Replay.DP1[replayCount] * 0.0000001, _cap1Replay.DP2[replayCount] * 0.0000001, "1");
                            mapPlot = 0;
                        }
                        mapPlot++;
                        replayCount++;
                        if (Countmax >= Countmax2)
                        {
                            double replay_tmp = ((double)replayCount) / (double)Countmax;
                            ReplayProgress = replay_tmp;
                        }
                        long time = replaySW.ElapsedMilliseconds;
                        double replayhz = (1000 / time)*2;
                        ReplayHz = replayhz.ToString();
                        replaySW.Restart();
                       
                    }
                    if (replayCount >= Countmax && Countmax >= Countmax2)
                    {

                        replayCount = 0;
                        replayCount2 = 0;
                        DoSomething?.Invoke(this, new NotificationEventArgs<string>("Clear"));
                        dataSeriesCap1G7.Clear();
                        dataSeriesCap1G8.Clear();
                        dataSeriesCap1G9X.Clear();
                        dataSeriesCap1G9Y.Clear();
                        dataSeriesCap1G9Z.Clear();
                        dataSeriesCap1XG13.Clear();
                        dataSeriesCap1YG13.Clear();
                        dataSeriesCap1ZG13.Clear();
                        dataSeriesCap2G14X.Clear();
                        dataSeriesCap2G14Y.Clear();
                        dataSeriesCap2G14Z.Clear();
                        dataSeriesCap1G15.Clear();
                        dataSeriesCap1G16.Clear();

                        dataSeriesCap2G7.Clear();
                        dataSeriesCap2G8.Clear();
//dataSeriesCap2G13.Clear();
                        dataSeriesCap2G14X.Clear();
                        dataSeriesCap2G14Y.Clear();
                        dataSeriesCap2G14Z.Clear();
                        dataSeriesCap2G15.Clear();
                        dataSeriesCap2G16.Clear();
                        // Clear Graphs
                        // Restarts the data replay
                    }
                }
                if (Cap2ReplayFile != null && Cap2ReplayFile != "") 
                {
                    if (_cap2Replay.Replay_Cap2_Ready)
                    {
                        Countmax2 = _cap2Replay.DP1.Length;

                        if (ReplayRestart)
                        {
                            replayCount2 = Countmax; // Sends to Restart Loop
                            ReplayRestart = false;
                        }
                        if (replayCount2 < Countmax && ReplayStart)
                        {
                            if (replayCount2 > 1000 && Countmax2 > Countmax && UpdateRange2 >= 100)
                            {
                                TimeRangeG7 = new DoubleRange(replayCount2 - 999, replayCount2 + 200);
                                TimeRangeG8 = new DoubleRange(replayCount2 - 999, replayCount2 + 200);
                                TimeRangeG9 = new DoubleRange(replayCount2 - 999, replayCount2 + 200);
                                TimeRangeG10 = new DoubleRange(replayCount2 - 999, replayCount2 + 200);
                                TimeRangeG11 = new DoubleRange(replayCount2 - 999, replayCount2 + 200);
                                TimeRangeG12 = new DoubleRange(replayCount2 - 999, replayCount2 + 200);
                                TimeRangeG13 = new DoubleRange(replayCount2 - 999, replayCount2 + 200);
                                TimeRangeG14 = new DoubleRange(replayCount2 - 999, replayCount2 + 200);
                                TimeRangeG15 = new DoubleRange(replayCount2 - 999, replayCount2 + 200);
                                TimeRangeG16 = new DoubleRange(replayCount2 - 999, replayCount2 + 200);
                            }
                            else if (Countmax2 > Countmax && UpdateRange2 >= 100)
                            {
                                TimeRangeG7 = new DoubleRange(0, replayCount2 + 200);
                                TimeRangeG8 = new DoubleRange(0, replayCount2 + 200);
                                TimeRangeG9 = new DoubleRange(0, replayCount2 + 200);
                                TimeRangeG10 = new DoubleRange(0, replayCount2 + 200);
                                TimeRangeG11 = new DoubleRange(0, replayCount2 + 200);
                                TimeRangeG12 = new DoubleRange(0, replayCount2 + 200);
                                TimeRangeG13 = new DoubleRange(0, replayCount2 + 200);
                                TimeRangeG14 = new DoubleRange(0, replayCount2 + 200);
                                TimeRangeG15 = new DoubleRange(0, replayCount2 + 200);
                                TimeRangeG16 = new DoubleRange(0, replayCount2 + 200);
                            }
                            UpdateRange2++;
                            // Add data to plots. Set timer tick to represent sample rate
                            dataSeriesCap2G7.Append(replayCount2, _cap2Replay.DP9[replayCount2]); // Alt
                            dataSeriesCap2G16.Append(replayCount2, _cap2Replay.DP3[replayCount2]); // GPS Alt
                            dataSeriesCap2G8.Append(replayCount2, _cap2Replay.DP4[replayCount2]); // Sat Count
                            dataSeriesCap2G14X.Append(replayCount2, _cap2Replay.DP6[replayCount2]); // Accel X
                            dataSeriesCap2G14Y.Append(replayCount2, _cap2Replay.DP7[replayCount2]); // Accel Y
                            dataSeriesCap2G14Z.Append(replayCount2, _cap2Replay.DP8[replayCount2]); // Accel Z
                            //dataSeriesCap2G13.Append(replayCount2, _cap2Replay.DP13[replayCount2]); // gyro X
                            //dataSeriesCap2G15.Append(replayCount2, _cap2Replay.DP8[replayCount2]); // Accel X Cap
                            replayCount2++;
                            if (replayCount2 < Countmax)
                            {
                                dataSeriesCap2G7.Append(replayCount2, _cap2Replay.DP9[replayCount2]); // Alt
                                dataSeriesCap2G16.Append(replayCount2, _cap2Replay.DP3[replayCount2]); // GPS Alt
                                dataSeriesCap2G8.Append(replayCount2, _cap2Replay.DP4[replayCount2]); // Sat Count
                                dataSeriesCap2G14X.Append(replayCount2, _cap2Replay.DP6[replayCount2]); // Accel X
                                dataSeriesCap2G14Y.Append(replayCount2, _cap2Replay.DP7[replayCount2]); // Accel Y
                                dataSeriesCap2G14Z.Append(replayCount2, _cap2Replay.DP8[replayCount2]); // Accel Z
                                //dataSeriesCap2G13.Append(replayCount2, _cap2Replay.DP13[replayCount2]); // gyro X
                                //dataSeriesCap2G15.Append(replayCount2, _cap2Replay.DP8[replayCount2]); // Accel X Cap2
                            }
                            

                            if (mapPlot2 >= 10)
                            {
                                AddLatLonReplay2(_cap2Replay.DP1[replayCount2] * 0.0000001, _cap2Replay.DP2[replayCount2] * 0.0000001, "1");
                                mapPlot2 = 0;
                            }
                            mapPlot2++;
                            replayCount2++;
                            if (Countmax2 > Countmax)
                            {
                                double replay_tmp = ((double)replayCount2) / (double)Countmax;
                                ReplayProgress = replay_tmp;
                            }

                        }
                        if (replayCount2 >= Countmax2 && Countmax2 > Countmax)
                        {
                            replayCount = 0;
                            replayCount2 = 0;
                            DoSomething2?.Invoke(this, new NotificationEventArgs<string>("Clear"));
                            dataSeriesCap1G7.Clear();
                            dataSeriesCap1G8.Clear();
                            dataSeriesCap1G9X.Clear();
                            dataSeriesCap1G9Y.Clear();
                            dataSeriesCap1G9Z.Clear();
                            dataSeriesCap1XG13.Clear();
                            dataSeriesCap1YG13.Clear();
                            dataSeriesCap1ZG13.Clear();
                            dataSeriesCap2G14X.Clear();
                            dataSeriesCap2G14Y.Clear();
                            dataSeriesCap2G14Z.Clear();
                            dataSeriesCap2G15.Clear();
                            dataSeriesCap1G16.Clear();

                            dataSeriesCap2G7.Clear();
                            dataSeriesCap2G8.Clear();
                            //dataSeriesCap2G13.Clear();
                            dataSeriesCap2G14X.Clear();
                            dataSeriesCap2G14Y.Clear();
                            dataSeriesCap2G14Z.Clear();
                            dataSeriesCap2G15.Clear();
                            dataSeriesCap2G16.Clear();
                            // Clear Graphs
                            // Restarts the data replay
                        }
                    }
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
            set { apogeeAlt = value; OnPropertyChanged("ApogeeAlt"); }
        }
        public string SysPressure1
        {
            get { return sysPressure1; }
            set { sysPressure1 = value; OnPropertyChanged("SysPressure1"); }
        }
        public string SysPressure2
        {
            get { return sysPressure2; }
            set { sysPressure2 = value; OnPropertyChanged("SysPressure2"); }
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

        private string rSSICOM = null;
        public string RSSICOM
        {
            get { return rSSICOM; }
            set { rSSICOM = value; OnPropertyChanged("RSSICOM"); }
        }

        private string lostFrame_cap1= "0";
        public string LostFrame_Cap1
        {
            get { return lostFrame_cap1; }
            set { lostFrame_cap1 = value; OnPropertyChanged("LostFrame_Cap1"); }
        }

        private string lostFrame_cap2 = "0";
        public string LostFrame_Cap2
        {
            get { return lostFrame_cap2; }
            set { lostFrame_cap2 = value; OnPropertyChanged("LostFrame_Cap2"); }
        }

        private string dataRate_Cap1= "0";
        public string DataRate_Cap1
        {
            get { return dataRate_Cap1; }
            set { dataRate_Cap1 = value; OnPropertyChanged("DataRate_Cap1"); }
        }

        private string dataRate_Cap2 = "0";
        public string DataRate_Cap2
        {
            get { return dataRate_Cap2; }
            set { dataRate_Cap2 = value; OnPropertyChanged("DataRate_Cap2"); }
        }

        private string replayHz = "0";
        public string ReplayHz
        {
            get { return replayHz; }
            set { replayHz = value; OnPropertyChanged("ReplayHz"); }
        }

        private double replayProgress = 0;
        public double ReplayProgress
        {
            get { return replayProgress; }
            set { replayProgress = value; OnPropertyChanged("ReplayProgress"); }
        }

        public GMapMarker GmapsPoint
        {
            get { return marker; }
            set { marker = value; OnPropertyChanged("GmapsPoint"); }
        }

        #endregion
        
        #region Commands
        // Contains methods for each button command

        // Updates COM port list
        public void UpdateComPort(object obj)
        {

            ////throw new NotImplementedException();
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
            FillPV.RaiseCanExecuteChanged();
            EjectRocket.RaiseCanExecuteChanged();
            
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
        GetRSSI RSSI = new GetRSSI();
        public void OpenRSSICOM(object obj)
        {
            RSSI.OpenNewPort(RSSICOM);

        }

        public bool CanOpenRSSICOM(object obj)
        {
            return true;
        }

        // Sends Status Check command to receive one data frame from rocket TM
        public void StatCheckRocket(object obj)
        {
            // Sends Status Check command to TM to update view
            Rocket_Data.CommandStringTM3.Enqueue("transmit_data");

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
            Rocket_Data.CommandStringTM3.Enqueue("start");
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
            Cap2_Data.CommandStringTM2.Enqueue("start");
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
        private DataReplay_Cap2 _cap2Replay;
        public void StartReplay(object obj)
        {
            if (Cap1ReplayFile != null && Cap1ReplayFile != "")
            {
                _cap1Replay = new DataReplay_Cap1();
                _cap1Replay.RunDataPlayback(Cap1ReplayFile);
            }
            if (Cap2ReplayFile != null && Cap2ReplayFile != "")
            {
                _cap2Replay = new DataReplay_Cap2();
                _cap2Replay.RunDataPlayback(Cap2ReplayFile);
            }

            ReplayStart = true;
            ReplayStop = false;
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

        public void EjectPayload(object obj)
        {
            DialogResult Result = MessageBox.Show("Confirm Ejection","", MessageBoxButtons.YesNo);
            if (Result == DialogResult.Yes)
            {
                Rocket_Data.CommandStringTM3.Enqueue("eject");
            }
        }

        public bool CanEjectPayload(object obj)
        {
            return RocketLinkOpen;
        }

        public void FillPressure(object obj)
        {
            Rocket_Data.CommandStringTM3.Enqueue("fill");
        }

        public bool CanFillPressure(object obj)
        {
            return RocketLinkOpen;
        }
        #endregion

        // Mapping
        #region Mapping Functions
        // Need to figure out MVVM for GMaps
        GMapMarker marker = new GMapMarker(new PointLatLng(39.86113302187091, -83.6557333146190));
        private void AddLatLonReplay(double Lat, double Lon, string Cap_Num)
        {
            string builder = Lat.ToString() + "," + Lon.ToString() + "," + Cap_Num;
            DoSomething?.Invoke(this, new NotificationEventArgs<string>(builder));

        }

        private void AddLatLonReplay2(double Lat, double Lon, string Cap_Num)
        {
            string builder = Lat.ToString() + "," + Lon.ToString() + "," + Cap_Num;
            DoSomething2?.Invoke(this, new NotificationEventArgs<string>(builder));

        }

        private void AddLatLonCap1(double Lat, double Lon)
        {
            string builder = Lat.ToString() + "," + Lon.ToString();
            Cap1TrackEvent?.Invoke(this, new NotificationEventArgs<string>(builder));

        }

        private void AddLatLonCap2(double Lat, double Lon)
        {
            string builder = Lat.ToString() + "," + Lon.ToString();
            Cap2TrackEvent?.Invoke(this, new NotificationEventArgs<string>(builder));

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

        private XyDataSeries<double, double> dataSeriesCap1G2X;
        private XyDataSeries<double, double> dataSeriesCap1G2Y;
        private XyDataSeries<double, double> dataSeriesCap1G2Z;

        private XyDataSeries<double, double> dataSeriesCap2G3X;
        private XyDataSeries<double, double> dataSeriesCap2G3Y;
        private XyDataSeries<double, double> dataSeriesCap2G3Z;

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

        private DoubleRange timeRangeG2;
        public DoubleRange TimeRangeG2
        {
            get => timeRangeG2;
            set
            {
                timeRangeG2 = value;
                OnPropertyChanged(nameof(TimeRangeG2));
            }
        }

        private DoubleRange timeRangeG4;
        public DoubleRange TimeRangeG4
        {
            get => timeRangeG4;
            set
            {
                timeRangeG4 = value;
                OnPropertyChanged(nameof(TimeRangeG4));
            }
        }

        private DoubleRange timeRangeG5;
        public DoubleRange TimeRangeG5
        {
            get => timeRangeG5;
            set
            {
                timeRangeG5 = value;
                OnPropertyChanged(nameof(TimeRangeG5));
            }
        }

        private double dp_1;
        public double DP_2
        {
            get => dp_1;
            set
            {
                dp_1 = value;
                OnPropertyChanged(nameof(DP_2));
            }
        }

        private DoubleRange timeRangeG6;
        public DoubleRange TimeRangeG6
        {
            get => timeRangeG6;
            set
            {
                timeRangeG6 = value;
                OnPropertyChanged(nameof(TimeRangeG6));
            }
        }
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
            dataSeriesCap1G1.SeriesName = "Atmos";
            dataSeriesCap2G1.SeriesName = "Cam";
            dataSeriesRocketG1.SeriesName = "Rocket";
            dataSeriesCap1G1.AcceptsUnsortedData = true;
            dataSeriesCap2G1.AcceptsUnsortedData = true;
            dataSeriesRocketG1.AcceptsUnsortedData = true;

            dataSeriesCap1G2X = new XyDataSeries<double, double>();
            dataSeriesCap1G2Y = new XyDataSeries<double, double>();
            dataSeriesCap1G2Z = new XyDataSeries<double, double>();
            dataSeriesCap1G2X.SeriesName = "X";
            dataSeriesCap1G2Y.SeriesName = "Y";
            dataSeriesCap1G2Z.SeriesName = "Z";
            //dataSeriesRocketG2.SeriesName = "Z-Axis";
            dataSeriesCap1G2X.AcceptsUnsortedData = true;
            dataSeriesCap1G2Y.AcceptsUnsortedData = true;
            dataSeriesCap1G2Z.AcceptsUnsortedData = true;

            dataSeriesCap2G3X = new XyDataSeries<double, double>();
            dataSeriesCap2G3Y = new XyDataSeries<double, double>();
            dataSeriesCap2G3Z = new XyDataSeries<double, double>();
            dataSeriesCap2G3X.SeriesName = "X";
            dataSeriesCap2G3Y.SeriesName = "Y";
            dataSeriesCap2G3Z.SeriesName = "Z";

            //dataSeriesRocketG2.SeriesName = "Z-Axis";
            dataSeriesCap1G2X.AcceptsUnsortedData = true;
            dataSeriesCap1G2Y.AcceptsUnsortedData = true;
            dataSeriesCap1G2Z.AcceptsUnsortedData = true;

            dataSeriesCap1G3 = new XyDataSeries<double, double>();
            dataSeriesCap2G3 = new XyDataSeries<double, double>();
            dataSeriesRocketG3 = new XyDataSeries<double, double>();
            dataSeriesCap1G3.SeriesName = "Atmos";
            dataSeriesCap2G3.SeriesName = "Cam";
            //dataSeriesRocketG3.SeriesName = "Z-Axis";
            dataSeriesCap1G3.AcceptsUnsortedData = true;
            dataSeriesCap2G3.AcceptsUnsortedData = true;
            dataSeriesRocketG3.AcceptsUnsortedData = true;

            dataSeriesCap1G4 = new XyDataSeries<double, double>();
            dataSeriesCap2G4 = new XyDataSeries<double, double>();
            dataSeriesRocketG4 = new XyDataSeries<double, double>();
            dataSeriesCap1G4.SeriesName = "Atmos";
            dataSeriesCap2G4.SeriesName = "Cam";
            //dataSeriesRocketG4.SeriesName = "Z-Axis";
            dataSeriesCap1G4.AcceptsUnsortedData = true;
            dataSeriesCap2G4.AcceptsUnsortedData = true;
            dataSeriesRocketG4.AcceptsUnsortedData = true;

            dataSeriesCap1G5 = new XyDataSeries<double, double>();
            dataSeriesCap2G5 = new XyDataSeries<double, double>();
            dataSeriesRocketG5 = new XyDataSeries<double, double>();
            dataSeriesCap1G5.SeriesName = "Atmos";
            dataSeriesCap2G5.SeriesName = "Cam";
            //dataSeriesRocketG5.SeriesName = "Z-Axis";
            dataSeriesCap1G5.AcceptsUnsortedData = true;
            dataSeriesCap2G5.AcceptsUnsortedData = true;
            dataSeriesRocketG5.AcceptsUnsortedData = true;

            dataSeriesCap1G6 = new XyDataSeries<double, double>();
            
            
            dataSeriesCap1G6.SeriesName = "Atmos";
            
            
            dataSeriesCap1G6.AcceptsUnsortedData = true;
            
            

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
                DataSeries = dataSeriesCap1G2X,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph2Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G2Y,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });
            Graph2Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G2Z,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 0, 255, 0)
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
           
            //// Chart 4
            //Graph4Series.Add(new LineRenderableSeriesViewModel
            //{
            //    DataSeries = dataSeriesCap1G4,
            //    AntiAliasing = false,
            //    StrokeThickness = 1,
            //    ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
            //    Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            //});

            Graph4Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G3X,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });
            Graph4Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G3Y,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214),
            });
            Graph4Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G3Z,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 0, 255, 0),
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
            get => graph13Series;
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
        private ObservableCollection<IRenderableSeriesViewModel> graph16Series { get; set; }
        public ObservableCollection<IRenderableSeriesViewModel> Graph16Series
        {
            get => graph16Series;
            set
            {
                graph16Series = value;
                OnPropertyChanged(nameof(Graph16Series));
            }
        }

        private XyDataSeries<double, double> dataSeriesCap1G7;
        private XyDataSeries<double, double> dataSeriesCap2G7;

        private XyDataSeries<double, double> dataSeriesCap1G8;
        private XyDataSeries<double, double> dataSeriesCap2G8;

        private XyDataSeries<double, double> dataSeriesCap1G9X;
        private XyDataSeries<double, double> dataSeriesCap1G9Y;
        private XyDataSeries<double, double> dataSeriesCap1G9Z;

        private XyDataSeries<double, double> dataSeriesCap1G10;
        private XyDataSeries<double, double> dataSeriesCap2G10;

        private XyDataSeries<double, double> dataSeriesCap1G11;
        private XyDataSeries<double, double> dataSeriesCap2G11;

        private XyDataSeries<double, double> dataSeriesCap1G12;
        private XyDataSeries<double, double> dataSeriesCap2G12;

        private XyDataSeries<double, double> dataSeriesCap1XG13;
        private XyDataSeries<double, double> dataSeriesCap1YG13;
        private XyDataSeries<double, double> dataSeriesCap1ZG13;

        private XyDataSeries<double, double> dataSeriesCap2G14X;
        private XyDataSeries<double, double> dataSeriesCap2G14Y;
        private XyDataSeries<double, double> dataSeriesCap2G14Z;

        private XyDataSeries<double, double> dataSeriesCap1G15;
        private XyDataSeries<double, double> dataSeriesCap2G15;

        private XyDataSeries<double, double> dataSeriesCap1G16;
        private XyDataSeries<double, double> dataSeriesCap2G16;

        private DoubleRange timeRangeG7;
        public DoubleRange TimeRangeG7
        {
            get => timeRangeG7;
            set
            {
                timeRangeG7 = value;
                OnPropertyChanged(nameof(TimeRangeG7));
            }
        }

        private DoubleRange timeRangeG8;
        public DoubleRange TimeRangeG8
        {
            get => timeRangeG8;
            set
            {
                timeRangeG8 = value;
                OnPropertyChanged(nameof(TimeRangeG8));
            }
        }

        private DoubleRange timeRangeG9;
        public DoubleRange TimeRangeG9
        {
            get => timeRangeG9;
            set
            {
                timeRangeG9 = value;
                OnPropertyChanged(nameof(TimeRangeG9));
            }
        }

        private DoubleRange timeRangeG10;
        public DoubleRange TimeRangeG10
        {
            get => timeRangeG10;
            set
            {
                timeRangeG10 = value;
                OnPropertyChanged(nameof(TimeRangeG10));
            }
        }

        private DoubleRange timeRangeG11;
        public DoubleRange TimeRangeG11
        {
            get => timeRangeG11;
            set
            {
                timeRangeG11 = value;
                OnPropertyChanged(nameof(TimeRangeG11));
            }
        }

        private DoubleRange timeRangeG12;
        public DoubleRange TimeRangeG12
        {
            get => timeRangeG12;
            set
            {
                timeRangeG12 = value;
                OnPropertyChanged(nameof(TimeRangeG12));
            }
        }

        private DoubleRange timeRangeG13;
        public DoubleRange TimeRangeG13
        {
            get => timeRangeG13;
            set
            {
                timeRangeG13 = value;
                OnPropertyChanged(nameof(TimeRangeG13));
            }
        }

        private DoubleRange timeRangeG14;
        public DoubleRange TimeRangeG14
        {
            get => timeRangeG14;
            set
            {
                timeRangeG14 = value;
                OnPropertyChanged(nameof(TimeRangeG14));
            }
        }

        private DoubleRange timeRangeG15;
        public DoubleRange TimeRangeG15
        {
            get => timeRangeG15;
            set
            {
                timeRangeG15 = value;
                OnPropertyChanged(nameof(TimeRangeG15));
            }
        }

        private DoubleRange timeRangeG16;
        public DoubleRange TimeRangeG16
        {
            get => timeRangeG16;
            set
            {
                timeRangeG16 = value;
                OnPropertyChanged(nameof(TimeRangeG16));
            }
        }


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
            Graph16Series = new ObservableCollection<IRenderableSeriesViewModel>();


            // Payload
            dataSeriesCap1G7 = new XyDataSeries<double, double>();
            dataSeriesCap2G7 = new XyDataSeries<double, double>();
            dataSeriesCap1G7.SeriesName = "Atmos";
            dataSeriesCap2G7.SeriesName = "Cam";
            dataSeriesCap1G7.AcceptsUnsortedData = true;
            dataSeriesCap2G7.AcceptsUnsortedData = true;

            dataSeriesCap1G8 = new XyDataSeries<double, double>();
            dataSeriesCap2G8 = new XyDataSeries<double, double>();
            dataSeriesCap1G8.SeriesName = "Atmos";
            dataSeriesCap2G8.SeriesName = "Cam";
            dataSeriesCap1G8.AcceptsUnsortedData = true;
            dataSeriesCap2G8.AcceptsUnsortedData = true;

            dataSeriesCap1G9X = new XyDataSeries<double, double>();
            dataSeriesCap1G9Y = new XyDataSeries<double, double>();
            dataSeriesCap1G9Z = new XyDataSeries<double, double>();
            dataSeriesCap1G9X.SeriesName = "X";
            dataSeriesCap1G9Y.SeriesName = "Y";
            dataSeriesCap1G9Z.SeriesName = "Z";
            dataSeriesCap1G9X.AcceptsUnsortedData = true;
            dataSeriesCap1G9Y.AcceptsUnsortedData = true;
            dataSeriesCap1G9Z.AcceptsUnsortedData = true;

            dataSeriesCap1G10 = new XyDataSeries<double, double>();
            dataSeriesCap2G10 = new XyDataSeries<double, double>();
            dataSeriesCap1G10.SeriesName = "Atmos";
            dataSeriesCap2G10.SeriesName = "Cam";
            dataSeriesCap1G10.AcceptsUnsortedData = true;
            dataSeriesCap2G10.AcceptsUnsortedData = true;

            dataSeriesCap1G11 = new XyDataSeries<double, double>();
            dataSeriesCap2G11 = new XyDataSeries<double, double>();
            dataSeriesCap1G11.SeriesName = "Atmos";
            dataSeriesCap2G11.SeriesName = "Cam";
            dataSeriesCap1G11.AcceptsUnsortedData = true;
            dataSeriesCap2G11.AcceptsUnsortedData = true;

            dataSeriesCap1G12 = new XyDataSeries<double, double>();
            dataSeriesCap2G12 = new XyDataSeries<double, double>();
            dataSeriesCap1G12.SeriesName = "Atmos";
            dataSeriesCap2G12.SeriesName = "Cam";
            dataSeriesCap1G12.AcceptsUnsortedData = true;
            dataSeriesCap2G12.AcceptsUnsortedData = true;

            dataSeriesCap1XG13 = new XyDataSeries<double, double>();
            dataSeriesCap1YG13 = new XyDataSeries<double, double>();
            dataSeriesCap1ZG13 = new XyDataSeries<double, double>();
            dataSeriesCap1XG13.SeriesName = "X";
            dataSeriesCap1YG13.SeriesName = "Y";
            dataSeriesCap1ZG13.SeriesName = "Z";
            dataSeriesCap1XG13.AcceptsUnsortedData = true;
            dataSeriesCap1YG13.AcceptsUnsortedData = true;
            dataSeriesCap1ZG13.AcceptsUnsortedData = true;

            dataSeriesCap2G14X = new XyDataSeries<double, double>();
            dataSeriesCap2G14Y = new XyDataSeries<double, double>();
            dataSeriesCap2G14Z = new XyDataSeries<double, double>();
            dataSeriesCap2G14X.SeriesName = "X";
            dataSeriesCap2G14Y.SeriesName = "Y";
            dataSeriesCap2G14Z.SeriesName = "Z";
            dataSeriesCap2G14X.AcceptsUnsortedData = true;
            dataSeriesCap2G14Y.AcceptsUnsortedData = true;
            dataSeriesCap2G14Z.AcceptsUnsortedData = true;

            dataSeriesCap1G15 = new XyDataSeries<double, double>();
            dataSeriesCap2G15 = new XyDataSeries<double, double>();
            dataSeriesCap1G15.SeriesName = "Atmos";
            dataSeriesCap2G15.SeriesName = "Cam";
            dataSeriesCap1G15.AcceptsUnsortedData = true;
            dataSeriesCap2G15.AcceptsUnsortedData = true;

            dataSeriesCap1G16 = new XyDataSeries<double, double>();
            dataSeriesCap2G16 = new XyDataSeries<double, double>();
            dataSeriesCap1G16.SeriesName = "Atmos";
            dataSeriesCap2G16.SeriesName = "Cam";
            dataSeriesCap1G16.AcceptsUnsortedData = true;
            dataSeriesCap2G16.AcceptsUnsortedData = true;


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


            // Stroke = System.Windows.Media.Color.FromArgb(255, 0, 255, 0)
            // Chart 3
            Graph9Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G9X,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph9Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G9Y,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            Graph9Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G9Z,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 0, 255, 0)
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

            //Chart 5
            Graph13Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1XG13,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph13Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1YG13,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            Graph13Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1ZG13,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 0, 255, 0)
            });

            // Chart 5
            Graph14Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G14X,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph14Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G14Y,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

            Graph14Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G14Z,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 0, 255, 0)
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

            Graph16Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap1G16,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 0),
            });

            Graph16Series.Add(new LineRenderableSeriesViewModel
            {
                DataSeries = dataSeriesCap2G16,
                AntiAliasing = false,
                StrokeThickness = 1,
                ResamplingMode = SciChart.Data.Numerics.ResamplingMode.None,
                Stroke = System.Windows.Media.Color.FromArgb(255, 255, 0, 214)
            });

        }
        #endregion
    }
}
