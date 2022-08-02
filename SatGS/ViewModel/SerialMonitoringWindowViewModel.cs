using SatGS.Interface;
using SatGS.SateliteData;
using SatGS.Communication;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
namespace SatGS.ViewModel
{
    internal class SerialMonitoringWindowViewModel : NotifyPropertyChanged
    {
        public ObservableCollection<SateliteStatus> SerialDataList { get; set; }

        private object selectedItem;
        public object SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged();
            }
        }

        public SerialMonitoringWindowViewModel()
        {
            SerialDataList = new ObservableCollection<SateliteStatus>();
            SerialReceiver.Instance().PacketReceived += PacketReceived;
        }

        private void PacketReceived(object sender, byte[] e)
        {
            var status = SateliteStatusFactory.Create(e);

            Application.Current.Dispatcher.Invoke(() =>
            {
                SerialDataList.Add(status);
                SelectedItem = status;
            });
        }

        public void OnClick(object sender, RoutedEventArgs e)
        {
            if (SerialDataList.Count == 0) return;

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            using (var file = new StreamWriter(File.OpenWrite($"{desktopPath}\\{DateTime.Now.ToString("yy.MM.dd_HH.mm.ss")}.csv")))
            {
                file.WriteLine("Latitude,Longitude,Altitude,Roll,Pitch,Yaw,Temperature,Humidity");

                foreach(var status in SerialDataList)
                {
                    file.WriteLine($"{status.Latitude},{status.Longitude},{status.Altitude},{status.Roll},{status.Yaw},{status.Pitch},{status.Temperature},{status.Humidity}");
                }
            }

            MessageBox.Show("출력 완료");
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;

            list.ScrollIntoView(e.AddedItems[0]);
        }
    }
}
