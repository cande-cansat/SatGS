using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SatGS.Communication;
using SatGS.ObjectDetection;
using SatGS.SateliteData;
using SatGS.Interface;
using SatGS.PathFinder;

namespace SatGS.ViewModel
{
    internal class ImageInspectorViewModel : NotifyPropertyChanged
    {
        private BitmapSource currentImage;
        public BitmapSource CurrentImage
        {
            get => currentImage;
            set
            {
                currentImage = value;
                OnPropertyChanged();
            }
        }

        private int selectedIndex;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                selectedIndex = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SateliteImage> Images { get; }

        private TcpReceiver receiver;
        private ObjectDetector objectDetector;

        private event EventHandler<byte[]> PathCalculated;

        public ImageInspectorViewModel()
        {
            Images = new ObservableCollection<SateliteImage>();

            if (!Directory.Exists("Images"))
                Directory.CreateDirectory("Images");
            else
            {
                string[] directories = File.ReadAllLines("ImagePath.cfg");

                foreach(var dirPath in directories)
                {
                    if (!Directory.Exists(dirPath)) continue;
                    var directory = new DirectoryInfo(dirPath);
                    var masks = new[] { "*.png", "*.jpg", "jpeg" };
                    var files = masks.SelectMany(directory.EnumerateFiles);

                    foreach (var file in files)
                        Images.Add(new SateliteImage(file.FullName));
                }
            }

            receiver = TcpReceiver.Instance();
            receiver.PacketReceived += PacketReceived;

            objectDetector = ObjectDetector.Instance();
            openCvResults = new Dictionary<string, BitmapSource>();

            PathCalculated += TcpSender.Instance().PathCalculated;
        }

        void PacketReceived(object sender, SateliteImage e)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Images.Add(e);
                SelectedIndex = Images.Count - 1;
            });
        }


        ~ImageInspectorViewModel()
        {
          
        }

        private Dictionary<string, BitmapSource> openCvResults;

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listview = sender as ListView;
            listview.ScrollIntoView(e.AddedItems[0]);

            var img = e.AddedItems[0] as SateliteImage;

            var file = new StreamWriter("degrees.txt", true);

            if (!openCvResults.ContainsKey(img.Path))
            {
                if (objectDetector.DetectContourOfRedObjects(img.Path, out var contourRect, out var bitmapSource))
                {
                    // distance = -(a * sin(theta) / w - 1)

                    var rect = contourRect.Value;

                    var a = rect.Width;
                    const double widthOfMeter = 15;
                    const double w = 151;
                    const double distanceCalibration = 0.041;

                    var distance = -(a / w - 1) * widthOfMeter / a + distanceCalibration;
                    Console.WriteLine($"d: {distance}, tx: {rect.X + rect.Width / 2}, ty: {rect.Y + rect.Height / 2}");
                    /*
                    if(distance > 0)
                    {
                        var translator = new PixelToCoordinateTranslator(
                        rect.X + rect.Width / 2, rect.Y + rect.Height / 2, distance);

                        var result = translator.calcDegreeFromPixel();
                        file.WriteLine($"d: {result[0]}\nphi: {result[1]}\ntheta: {result[2]}");

                    }*/
                    // 여기서 PathCalculated를 Invoke

                    //PathCalculated?.Invoke();
                }

                openCvResults.Add(img.Path, bitmapSource);
                CurrentImage = bitmapSource;

                SaveOpenCVResult(img.FileName, bitmapSource);
            }
            else
            {
                CurrentImage = openCvResults[img.Path];
            }

            file.Flush();
            file.Close();
        }

        private void SaveOpenCVResult(string fileName, BitmapSource bitmapSource)
        {
            var opencvPath = "./Images/OpenCV";
            if (!Directory.Exists(opencvPath))
                Directory.CreateDirectory(opencvPath);

            using (var stream = new FileStream($"{opencvPath}/cv_{fileName}", FileMode.Create))
            {
                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);
            }
        }
    }
}
