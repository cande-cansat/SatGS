﻿using SatGS.Socket;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SatGS.ViewModel
{
    internal class SerialMonitoringWindowViewModel : Model.NotifyPropertyChanged
    {
        public ObservableCollection<Model.SatliteStatus2> SerialDataList { get; set; }

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

        public SerialMonitoringWindowViewModel()
        {
            SerialDataList = new ObservableCollection<Model.SatliteStatus2>();
            SerialReceiver.Instance().PacketReceived += PacketReceived;
        }

        private void PacketReceived(object sender, Model.SatliteStatus2 e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SerialDataList.Add(e);
                SelectedIndex = SerialDataList.Count - 1;
                
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
    }
}
