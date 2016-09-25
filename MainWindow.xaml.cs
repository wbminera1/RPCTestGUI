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
        public MainWindow()
        {
            InitializeComponent();
        }
        void DrawRubbish(Image target)
        {
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                Random rand = new Random();

                for (int i = 0; i < 200; i++)
                    dc.DrawRectangle(Brushes.Red, null, new Rect(rand.NextDouble() * target.Width, rand.NextDouble() * target.Height, 1, 1));

                dc.Close();
            }
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)target.Width, (int)target.Height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            target.Source = rtb;
        }
        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                try
                {
                    bf.Serialize(ms, obj);
                }
                catch (SerializationException)
                {

                }
                return ms.ToArray();
            }
        }

        void HexDumpToConsole(object obj)
        {
            byte[] bytes = ObjectToByteArray(obj);
            ToConsole(BitConverter.ToString(bytes));
        }

        void HexDumpToConsole(int val)
        {
            ToConsole(BitConverter.ToString(BitConverter.GetBytes(val)));
        }

        void ToConsole(string str)
        {
            Console.WriteLine(str);
            textConsole.AppendText(ConsoleStrCount + " " +str + "\n");
            textConsole.ScrollToEnd();
            ++ConsoleStrCount;
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            DrawRubbish(MainImage);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            Client client = new Client();
            Console.WriteLine("Thread started");
        }

        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            ToConsole("Text");
            HexDumpToConsole(32);
        }

        private int ConsoleStrCount = 0;

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            RPCCommand.Test();
        }
    }
}
