﻿using SatGS.Socket;
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

        public MainWindowViewModel()
        {
        }

        public void OnClosed(object sender, EventArgs e)
        {
            TcpReceiver.Instance().CleanUpSocket();
            SerialReceiver.Instance().CleanUpSerial();
        }
    }
}
