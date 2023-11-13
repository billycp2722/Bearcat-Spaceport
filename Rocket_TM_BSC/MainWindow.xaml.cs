using SciChart.Charting3D.Model;
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

        public static Stream StringToStream(string src)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(src);
            return new MemoryStream(byteArray);
        }

    }
}
