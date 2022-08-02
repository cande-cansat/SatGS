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
                string[] directories = File.ReadAllLines(
#if DEBUG
                    "../../../ImagePath.cfg"
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

        void PacketReceived(object sender, SateliteImage e)
        {
            /*
            Application.Current.Dispatcher.Invoke(() =>
            {
                Images.Add(image);
                SelectedIndex = Images.Count - 1;
            });
            */
            /*
            Application.Current.Dispatcher.BeginInvoke((Action) (() => {
                
            }));
            */
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Images.Add(e);
                SelectedIndex = Images.Count - 1;
            });

            PathCalculator calculator = new PathCalculator();

            // 여기서 이미지 내의 물체의 path를 구한다.

            /*{
                var coordinates = calculator.calcPath();

                var size = coordinates.Count * 3 * 4;
                var buffer = new byte[size];
                var offset = 0;
                foreach (var coordinate in coordinates)
                {
                    var bItem1 = BitConverter.GetBytes(coordinate.item1);
                    var bItem2 = BitConverter.GetBytes(coordinate.item2);
                    var bItem3 = BitConverter.GetBytes(coordinate.item3);

                    bItem1.CopyTo(buffer, offset); offset += 4;
                    bItem2.CopyTo(buffer, offset); offset += 4;
                    bItem3.CopyTo(buffer, offset); offset += 4;
                }

                PathCalculated?.Invoke(this, buffer);
            }*/
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
            
            if (!openCvResults.ContainsKey(img.Path))
            {
                var detected = objectDetector.DetectContourOfRedObjects(img.Path);
                if (detected == null) return;

                openCvResults.Add(img.Path, detected);
                CurrentImage = openCvResults[img.Path];


                // 이 부분에서 OpenCV Output image를 파일로 출력해야 함

                var opencvPath = "./Images/OpenCV";
                if (!Directory.Exists(opencvPath))
                    Directory.CreateDirectory(opencvPath);

                using(var stream = new FileStream($"{opencvPath}/cv_{img.FileName}", FileMode.Create))
                {
                    var encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(openCvResults[img.Path]));
                    encoder.Save(stream);
                }
            }
            else
            {
                CurrentImage = openCvResults[img.Path];
            }
        }
    }
}
