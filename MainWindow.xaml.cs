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
    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

 


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
            m_Draw.TestLine();
            BitmapSource bs = m_Draw.RawToBitmap();
            MainImage.Source = bs;
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
            ClientCommandHandler client = new ClientCommandHandler(m_DebugConsole);
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
    }
}
