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
        public ObservableCollection<SateliteImage> Images { get; }

        private TcpReceiver receiver;
        private ObjectDetector objectDetector;

        private event EventHandler<List<Coordinate>> PathCalculated;

        public ImageInspectorViewModel()
        {
            Images = new ObservableCollection<SateliteImage>();

            if (!Directory.Exists("Images"))
                Directory.CreateDirectory("Images");
            else
            {
                string[] directories = File.ReadAllLines(
#if DEBUG
                    "../../ImagePath.cfg"
#else
                    "ImagePath.cfg"
#endif
                    );

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

        void PacketReceived(object sender, byte[] e)
        {
            var image = SateliteImageFactory.Create(e);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Images.Add(image);
            });

            PathCalculator calculator = new PathCalculator();

            // 여기서 이미지 내의 물체의 path를 구한다.

            PathCalculated?.Invoke(this, calculator.calcPath());
        }


        ~ImageInspectorViewModel()
        {
          
        }

        private Dictionary<string, BitmapSource> openCvResults;

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var imgPath = (e.AddedItems[0] as SateliteImage).Path;

            if (!openCvResults.ContainsKey(imgPath))
                openCvResults.Add(imgPath, objectDetector.DetectContourOfRedObjects(imgPath));
                //openCvResults.Add(imgPath, openCv.ContourDetectionFromImage(imgPath));

            CurrentImage = openCvResults[imgPath];
        }
    }
}
