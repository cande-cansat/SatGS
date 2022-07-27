using SatGS.Socket;
using SatGS.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Layout;

namespace SatGS.ViewModel
{
    internal class MainWindowViewModel
    {

        SerialMonitoringWindow serialMonitoringWindow;

        public MainWindowViewModel()
        {
            serialMonitoringWindow = new SerialMonitoringWindow();
            serialMonitoringWindow.Show();
        }

        public void OnClosed(object sender, EventArgs e)
        {
            serialMonitoringWindow.Close();
            TcpReceiver.Instance().CleanUpSocket();
            SerialReceiver.Instance().CleanUpSerial();
        }
    }
}
