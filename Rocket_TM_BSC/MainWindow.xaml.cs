﻿using SciChart.Charting3D.Model;
using SciChart.Charting3D.Visuals.Object;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SciChart.Charting3D;
using System.Windows.Controls;
using System.Windows.Media;
using SciChart.Core.Extensions;
using SharpDX;
using System.Windows.Input;
using GMap.NET.MapProviders;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Windows.Media.Imaging;


namespace Rocket_TM_BSC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //var obj = new ObjectModel3D();
            //sciChart3DSurface.SceneObjects.Add(obj);
            //obj.Position = new Vector3(0f, 0f, 0f);
            //obj.CoordinateMode = ObjectCoordinateMode.Relative;
            //obj.Scale = new Vector3(2f, 2f, 2f);

            //var streamForObject = StringToStream(_stringForStream);
            //var objModelSource = new ObjectModelSource(streamForObject);
            //obj.Source = objModelSource;

            


        }

        //private void OnClickHorizontalRotate(object sender, RoutedEventArgs e)
        //{
        //    Camera3D.OrbitalYaw = ((int)Camera3D.OrbitalYaw < 360) ? Camera3D.OrbitalYaw + 90 : (Camera3D.OrbitalYaw - 360) * (-1);
        //}

        //private void OnClickVerticalRotate(object sender, RoutedEventArgs e)
        //{
        //    Camera3D.OrbitalPitch = ((int)Camera3D.OrbitalPitch < 89) ? Camera3D.OrbitalPitch + 90 : -90;
        //}

        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            // choose your provider here
            mapView.MapProvider = GMap.NET.MapProviders.GoogleHybridMapProvider.Instance;
            mapView.Position = new GMap.NET.PointLatLng(39.86113302187091, -83.6557333146190);
            mapView.MinZoom = 2;
            mapView.MaxZoom = 17;
            mapView.CacheLocation = "C:\\Users\\Public\\Desktop";

            // whole world zoom
            mapView.Zoom = 14;
            // lets the map use the mousewheel to zoom
            mapView.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            // lets the user drag the map
            mapView.CanDragMap = true;
            // lets the user drag the map with the left mouse button
            mapView.DragButton = MouseButton.Left;
            GMapMarker marker = new GMapMarker(new PointLatLng(39.86113302187091, -83.6557333146190));
            marker.Shape = new Image
            {
                Width = 100,
                Height = 100,
                Source = new BitmapImage(new System.Uri("pack://application:,,,/assets/Bearcat_Spaceport_Cup_Logo.png"))
            };

            mapView.Markers.Add(marker);
            GMapMarker marker2 = new GMapMarker(new PointLatLng(39.82821816857987, -83.63717981659063));
            marker2.Shape = new Image
            {
                Width = 50,
                Height = 50,
                Source = new BitmapImage(new System.Uri("pack://application:,,,/assets/Bearcat_Spaceport_Cup_Logo.png"))
            };
            mapView.Markers.Add(marker2);

        }

        public static Stream StringToStream(string src)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(src);
            return new MemoryStream(byteArray);
        }

    }
}
