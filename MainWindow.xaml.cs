using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace FrontEnd
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DebugConsole m_DebugConsole;
        private List<ClientCommandHandler> m_Clients = new List<ClientCommandHandler>();
        private Draw m_Draw;

        public MainWindow()
        {
            InitializeComponent();
            ConnectButton.Background = Brushes.Green;
            m_DebugConsole = new DebugConsole(textConsole);
            m_Draw = new Draw();
            m_Draw.Init((int)MainImage.Width, (int)MainImage.Height, 4);
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            //m_Draw.TestSetPixel();
            //m_Draw.TestLine();
            //BitmapSource bs = m_Draw.RawToBitmap();
//             RayTrace.Sphere.Test();
//             RayTrace.RayTrace rt = new RayTrace.RayTrace();
//             RayTrace.RGBImage image = rt.Trace((int)MainImage.Width, (int)MainImage.Height);
//             BitmapSource bs = image.RawToBitmap();
//             MainImage.Source = bs;
        }

        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            m_DebugConsole.WriteLine("Text");
            m_DebugConsole.HexDumpToConsole(32);
        }
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            RPCCommand.Test();
        }
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ClientCommandHandler client = new ClientCommandHandler();
            if (client.WaitForConnection())
            {
                m_Clients.Add(client);
                client.Send(new RPCCommandConnect(0x0001));
                m_DebugConsole.WriteLine("Thread started");
                ConnectButton.Background = Brushes.Gray;
            }
            else
            {
                m_DebugConsole.WriteLine("Thread not started");
            }
        }
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if(m_Clients.Count > 0)
            {
                int idx = m_Clients.Count - 1;
                m_Clients[idx].Stop();
                m_Clients.RemoveAt(idx);
            }
        }
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            new Thread(RunAgent).Start();
        }
        private void RunAgent()
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = "args";
            start.FileName = @"C:\Temp\TAgent.exe";
            //start.WindowStyle = ProcessWindowStyle.Hidden;
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.CreateNoWindow = true;
            int exitCode;
            using (Process proc = Process.Start(start))
            {
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }
        }
    }
}
