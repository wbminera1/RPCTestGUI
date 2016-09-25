using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Client
{
    private TcpClient client;

    private Receiver m_Receiver;
    private Sender m_Sender;
    private Thread m_Thread;
    private readonly object m_SendLock = new object();

    internal class Receiver
    {

        internal Receiver(Client cbase)
        {
            m_Base = cbase;
            m_Buffer = new byte[4096];
            m_ThreadRec = new Thread(Run);
            m_ThreadRec.Start();
        }

        private void Run()
        {
            // main thread loop for receiving data...
            m_Base.client.GetStream().Read(m_Buffer, 0, m_Buffer.Length);
        }

        private Thread m_ThreadRec;
        private Client m_Base;
        private byte[] m_Buffer;
    }

    internal class Sender
    {

        internal Sender(Client cbase)
        {
            m_Base = cbase;
            m_ThreadSend = new Thread(Run);
            m_ThreadSend.Start();
        }

        private void Run()
        {
            // main thread loop for sending data...
            lock (m_Base.m_SendLock)
            {
                while (true)
                {
                    Monitor.Wait(m_Base.m_SendLock);
                    m_Base.client.GetStream().Write(m_Buffer, 0, m_Buffer.GetLength(0));
                }
            }
        }
        public bool Send(byte[] data)
        {
            lock (m_Base.m_SendLock)
            {
                m_Buffer = data;
                Monitor.Pulse(m_Base.m_SendLock);
            }
            return true;
        }

        private Thread m_ThreadSend;
        private Client m_Base;
        private byte[] m_Buffer;
    }

    public Client()
    {
        this.client = new TcpClient();
        this.m_Thread = new Thread(Run);
        this.m_Thread.Start();
    }

    // This method that will be called when the thread is started
    public void Run()
    {
        try
        {
            this.client.Connect("127.0.0.1", 9999);
            this.m_Sender = new Sender(this);
            this.m_Receiver = new Receiver(this);
            //this.sender.SendData(Encoding.ASCII.GetBytes("hello"));
            while (true)
            {
                try
                {
                    //Console.WriteLine("Running in its own thread.");
                    Thread.Sleep(100);
                    //client.BeginConnect()
                }
                catch (ThreadAbortException)
                {
                    Console.Write("ThreadAbortException");
                }
            }
        }
        catch (System.Net.Sockets.SocketException)
        {
            Console.WriteLine("SocketException " + this.client.ToString());
        }


    }

    public virtual bool Send(byte[] data)
    {
        return m_Sender.Send(data);
    }

    public virtual int Receive(byte[] data)
    {
        return 0;
    }

};


