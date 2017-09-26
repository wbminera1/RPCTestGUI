using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Client
{
    private TcpClient m_Client;
    private TcpListener m_Listener;

    private string m_Hostname;
    private int m_Port;
    private bool m_IsServer;

    private Receiver m_Receiver;
    private Sender m_Sender;
    private Thread m_Thread;
    private readonly object m_SendLock = new object();
    private volatile bool m_IsStarted = false;
    private volatile bool m_IsDisconnected = false;
    private volatile bool m_Stop = false;

    //private FrontEnd.DebugConsole m_DebugConsole;


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
            m_Started = true;
            while (true)
            {
                try
                {
                    int read = m_Base.m_Client.GetStream().Read(buffer, 0, buffer.Length);
                    if (read > 0)
                    {
/*
                        if(m_Base.m_DebugConsole != null)
                        {
                            lock (m_Base.m_DebugConsole)
                            {
                                byte[] arr = new byte[read];
                                Array.Copy(buffer, arr, read);
                                m_Base.m_DebugConsole.WriteLine(arr);
                            }
                        }
*/
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
        public volatile bool m_Started = false;
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
                m_Started = true;
                while (true)
                {
                    Monitor.Wait(m_Base.m_SendLock);
/*
                    if (m_Base.m_DebugConsole != null)
                    {
                        lock (m_Base.m_DebugConsole)
                        {
                            byte[] arr = new byte[m_Stream.Length];
                            Array.Copy(m_Stream.GetBuffer(), arr, m_Stream.Length);
                            m_Base.m_DebugConsole.WriteLine(arr);
                        }
                    }
*/
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
        public volatile bool m_Started = false;
    }
    public Client(/*FrontEnd.DebugConsole debugConsole*/)
    {
        //m_DebugConsole = debugConsole;
    }
    public void Connect(string hostname, int port)
    {
        m_Hostname = hostname;
        m_Port = port;
        m_IsServer = false;
        m_Thread = new Thread(Run);
        m_Thread.Start();
    }
    public void Listen(int port)
    {
        m_Hostname = "";
        m_Port = port;
        m_IsServer = true;
        m_Thread = new Thread(Run);
        m_Thread.Start();
    }
    private void Run()
    {
        m_IsStarted = false;
        m_Stop = false;
        m_IsDisconnected = false;
        try
        {
            if(m_IsServer)
            {
                Console.WriteLine("Listening...");
                var al = Dns.GetHostEntry("localhost").AddressList;
                foreach(var addr in al)
                {
                    if(addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        m_Listener = new TcpListener(addr, m_Port);
                        Console.WriteLine("Listening on " + addr.ToString());
                        m_Listener.Start(1);
                        m_Client = m_Listener.AcceptTcpClient();
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Connecting...");
                m_Client = new TcpClient();
                m_Client.Connect(m_Hostname, m_Port);
            }
            Console.WriteLine("Connected");
            m_Sender = new Sender(this);
            m_Receiver = new Receiver(this);
            while (!m_Sender.m_Started || !m_Receiver.m_Started)
            {
                Thread.Sleep(10);
            }
            m_IsStarted = true;
            while (!m_Stop)
            {
                try
                {
                    Thread.Sleep(100);
                }
                catch (ThreadAbortException)
                {
                    Console.WriteLine("ThreadAbortException");
                    m_IsDisconnected = true;
                }
            }
            m_Client.Close();
            Console.WriteLine("Client Stopping");
        }
        catch (SocketException)
        {
            Console.WriteLine("SocketException " + this.m_Client.ToString());
            m_IsDisconnected = true;
        }
        m_IsStarted = false;
    }

    public bool WaitForConnection()
    {
        while (!m_IsStarted)
        {
            if (m_IsDisconnected)
            {
                return false;
            }
            Thread.Sleep(100);
        }
        return true;
    }
    public void Stop()
    {
        m_Stop = true;
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


