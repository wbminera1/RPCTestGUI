using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Client
{
    private TcpClient m_Client;

    private Receiver m_Receiver;
    private Sender m_Sender;
    private Thread m_Thread;
    private readonly object m_SendLock = new object();
    //private readonly object m_StartLock = new object();
    private volatile bool m_IsStarted = false;

    private FrontEnd.DebugConsole m_DebugConsole;


    internal class Receiver
    {

        internal Receiver(Client cbase)
        {
            m_Base = cbase;
            m_ThreadRec = new Thread(Run);
            m_ThreadRec.Start();
        }

        private void Run()
        {
            byte[] buffer = new byte[1024];
            while(true)
            {
                try
                {
                    int read = m_Base.m_Client.GetStream().Read(buffer, 0, buffer.Length);
                    if(read > 0)
                    {
                        lock (m_Base.m_DebugConsole)
                        {
                            byte[] arr = new byte[read];
                            Array.Copy(buffer, arr, read);
                            m_Base.m_DebugConsole.ToConsole(arr);
                        }
                    }
                }
                catch (System.IO.IOException)
                {
                    break;
                }
                catch (System.InvalidOperationException)
                {
                    break;
                }
            }
        }

        private Thread m_ThreadRec;
        private Client m_Base;
    }

    internal class Sender
    {

        internal Sender(Client cbase)
        {
            m_Base = cbase;
            m_Stream = new MemoryStream();
            m_ThreadSend = new Thread(Run);
            m_ThreadSend.Start();
        }

        private void Run()
        {
            lock (m_Base.m_SendLock)
            {
                while (true)
                {
                    Monitor.Wait(m_Base.m_SendLock);
                    lock (m_Base.m_DebugConsole)
                    {
                        byte[] arr = new byte[m_Stream.Length];
                        Array.Copy(m_Stream.GetBuffer(), arr, m_Stream.Length);
                        m_Base.m_DebugConsole.ToConsole(arr);
                    }
                    m_Stream.Seek(0, SeekOrigin.Begin);
                    m_Stream.CopyTo(m_Base.m_Client.GetStream());
                    m_Stream.SetLength(0);
                }
            }
        }

        public bool SendWithSize(MemoryStream mstr)
        {
            lock (m_Base.m_SendLock)
            {
                byte[] size = BitConverter.GetBytes((UInt32)mstr.Length);
                m_Stream.Write(size, 0, size.Length);
                mstr.Seek(0, SeekOrigin.Begin);
                mstr.CopyTo(m_Stream);
                Monitor.Pulse(m_Base.m_SendLock);
            }
            return true;
        }

        public bool Send(byte[] data)
        {
            lock (m_Base.m_SendLock)
            {
                m_Stream.Write(data, 0, data.Length);
                Monitor.Pulse(m_Base.m_SendLock);
            }
            return true;
        }

        public bool AddToSend(byte[] data)
        {
            lock (m_Base.m_SendLock)
            {
                m_Stream.Write(data, 0, data.Length);
            }
            return true;
        }

        private Thread m_ThreadSend;
        private Client m_Base;
        private MemoryStream m_Stream;
    }

    public Client(FrontEnd.DebugConsole debugConsole)
    {
        m_Client = new TcpClient();
        m_Thread = new Thread(Run);
        m_Thread.Start();
        m_DebugConsole = debugConsole;

}

public void Run()
    {
        try
        {
            m_Client.Connect("127.0.0.1", 9999);
            m_Sender = new Sender(this);
            m_Receiver = new Receiver(this);
            m_IsStarted = true;
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
            Console.WriteLine("SocketException " + this.m_Client.ToString());
        }
        m_IsStarted = false;
    }

    public void WaitForConnection()
    {
        while(!m_IsStarted)
        {
            Thread.Sleep(0);
        }
    }

    public virtual bool Send(byte[] data)
    {
        return m_Sender.Send(data);
    }

    public virtual bool SendWithSize(MemoryStream mstr)
    {
        return m_Sender.SendWithSize(mstr);
    }

    public virtual int Receive(byte[] data)
    {
        return 0;
    }

};


