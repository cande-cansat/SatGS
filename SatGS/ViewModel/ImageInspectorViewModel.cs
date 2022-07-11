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
using SatGS.Model;
using SatGS.OpenCV;
using SatGS.Socket;

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
        public ObservableCollection<ImagePath> Images { get; }

        private Receiver receiver;
        private OpenCV.OpenCV openCv;

        public ImageInspectorViewModel()
        {
            Images = new ObservableCollection<ImagePath>();

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
                    var directory = new DirectoryInfo(dirPath);
                    var masks = new[] { "*.png", "*.jpg", "jpeg" };
                    var files = masks.SelectMany(directory.EnumerateFiles);

                    foreach (var file in files)
                        Images.Add(new ImagePath(file.FullName));
                }
            }

            receiver = Receiver.Instance();
            receiver.PacketReceived += PacketReceived;

            openCv = OpenCV.OpenCV.Instance();
            openCvResults = new Dictionary<string, BitmapSource>();
        }

        void PacketReceived(object sender, PacketData e)
        {
            if (e.Data[0] != 1) return;

            var image = Factory.SatliteImageFactory.Create(e);

            var fileName = $"{DateTime.Now:MM.dd_HH.mm.ss}";
            var path = $"{Directory.GetCurrentDirectory()}\\Images\\{fileName}.png";
            image.Save(path, ImageFormat.Png);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Images.Add(new ImagePath(path));
            });
        }


        ~ImageInspectorViewModel()
        {
          
        }

        private Dictionary<string, BitmapSource> openCvResults;

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var imgPath = ((ImagePath)e.AddedItems[0]).FullPath;

            if (!openCvResults.ContainsKey(imgPath))
                //openCvResults.Add(imgPath, openCv.DetectionWithYolo3(imgPath));
                openCvResults.Add(imgPath, openCv.FindCountour(imgPath));
            
            CurrentImage = openCvResults[imgPath];
        }
    }
}
