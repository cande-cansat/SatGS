using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using SatGS.Communication;
using SatGS.SateliteData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using SatGS.Interface;

namespace SatGS.ViewModel
{
    internal class MapViewModel : NotifyPropertyChanged
    {
        public object GMapControl { get; set; }

        public MapViewModel()
        {
            // x86_64용, ARM 지원 안함
            InitializeGMap();
        }

        private void InitializeGMap()
        {
            var gMapControl = new GMapControl();
            gMapControl.MapProvider = GMapProviders.GoogleSatelliteMap;

            gMapControl.MinZoom = 6;
            gMapControl.MaxZoom = 6;
            gMapControl.Position = new GMap.NET.PointLatLng(37.541, 126.986);
            gMapControl.Zoom = 6;

            GMapControl = gMapControl;
            SerialReceiver.Instance().PacketReceived += PacketReceived;
        }

        private void PacketReceived(object sender, SateliteStatus e)
        {
            GMapMarker marker;
            var newPosition = new GMap.NET.PointLatLng(e.Latitude, e.Longitude);

            var gMapControl = GMapControl as GMapControl;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (gMapControl.Markers.Count == 0)
                {
                    marker = new GMapMarker(newPosition)
                    {
                        Shape = new Ellipse()
                        {
                            Width = 10,
                            Height = 10,
                            Stroke = Brushes.Red,
                            StrokeThickness = 2,
                            Fill = Brushes.White
                        }
                    };
                    gMapControl.Markers.Add(marker);
                }
                else
                {
                    marker = gMapControl.Markers.First();
                    marker.Position = newPosition;
                }
            });
        }
    }
}
