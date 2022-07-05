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
using SatGS.Socket;

namespace SatGS.ViewModel
{
    internal class ImageInspectorViewModel : NotifyPropertyChanged
    {
        private BitmapImage currentImage;
        public BitmapImage CurrentImage
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

        public ImageInspectorViewModel()
        {
            Images = new ObservableCollection<ImagePath>();

            if (!Directory.Exists("Images"))
                Directory.CreateDirectory("Images");
            else
            {
                var directory = new DirectoryInfo(@".\Images\");
                var masks = new[] { "*.png", "*.jpg", "jpeg" };
                var files = masks.SelectMany(directory.EnumerateFiles);

                foreach (var file in files)
                    Images.Add(new ImagePath(file.FullName));
            }

            receiver = Receiver.Instance();

            receiver.PacketReceived += PacketReceived;
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


        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentImage = new BitmapImage(new Uri(((ImagePath)e.AddedItems[0]).FullPath));
        }
    }
}
