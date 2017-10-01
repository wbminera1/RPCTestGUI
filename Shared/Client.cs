using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Client
{
    public delegate void OnReceived(byte[] data);
    public delegate void OnClosed();

    private TcpClient m_Client;
    private TcpListener m_Listener;

    private string m_Hostname;
    private int m_Port;
    private bool m_IsServer;

    private Receiver m_Receiver;
    private Sender m_Sender;
    private Thread m_Thread;
    private readonly object m_SendLock = new object();
    private readonly object m_ConnectLock = new object();
    private volatile bool m_IsStarted = false;
    private volatile bool m_IsDisconnected = false;

    protected OnReceived m_OnReceived;
    protected OnClosed m_OnClosed;

    internal class Receiver
    {
        internal Receiver(Client cbase)
        {
            m_Base = cbase;
            m_ThreadRec = new Thread(Run);
            m_ThreadRec.Name = "Client.Receiver";
            m_ThreadRec.Start();
        }
        private void Run()
        {
            Console.WriteLine("Receiver started");
            byte[] buffer = new byte[1024];
            m_Started = true;
            while (true)
            {
                try
                {
                    int read = m_Base.m_Client.GetStream().Read(buffer, 0, buffer.Length);
                    Console.WriteLine("Receiver - read " + read);
                    if (read <= 0)
                    {
                        break;
                    }
                    m_Base.m_OnReceived?.Invoke(buffer);
                }
                catch (Exception)
                {
                    break;
                }
            }
            m_Base.Stop();
            m_Started = false;
            Console.WriteLine("Receiver stopped");
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
            m_ThreadSend.Name = "Client.Sender";
            m_ThreadSend.Start();
        }
        private void Run()
        {
            Console.WriteLine("Sender started");
            lock (m_Base.m_SendLock)
            {
                m_Started = true;
                while (m_Started)
                {
                    Monitor.Wait(m_Base.m_SendLock);
                    if(m_Stream.Length > 0)
                    {
                        m_Stream.Seek(0, SeekOrigin.Begin);
                        m_Stream.CopyTo(m_Base.m_Client.GetStream());
                        m_Stream.SetLength(0);
                    }
                }
            }
            Console.WriteLine("Sender stopped");
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
        public void Stop()
        {
            if(m_Started)
            {
                lock (m_Base.m_SendLock)
                {
                    m_Started = false;
                    m_Stream.SetLength(0);
                    Monitor.Pulse(m_Base.m_SendLock);
                }
            }
        }

        private Thread m_ThreadSend;
        private Client m_Base;
        private MemoryStream m_Stream;
        public volatile bool m_Started = false;
    }
    public Client(OnReceived onReceived = null, OnClosed onClosed = null)
    {
        m_OnReceived = onReceived;
        m_OnClosed = onClosed;
    }
    public void Connect(string hostname, int port)
    {
        m_Hostname = hostname;
        m_Port = port;
        m_IsServer = false;
        m_Thread = new Thread(Run);
        m_Thread.Name = "Client.Connect";
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
                Thread.Yield();
            }
            try
            {
                lock (m_ConnectLock)
                {
                    m_IsStarted = true;
                    Console.WriteLine("Waiting...");
                    Monitor.Wait(m_ConnectLock);
                    Console.WriteLine("End waiting");
                }
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine("ThreadAbortException");
                m_IsDisconnected = true;
            }
            m_Client.Close();
            m_Sender.Stop();
            Console.WriteLine("Client Stopped");
        }
        catch (SocketException)
        {
            Console.WriteLine("SocketException " + this.m_Client.ToString());
            m_IsDisconnected = true;
        }
        m_IsStarted = false;
        m_OnClosed?.Invoke();
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
        lock(m_ConnectLock)
        {
            Monitor.Pulse(m_ConnectLock);
        }
    }
    public void WaitForEnd()
    {
        Console.WriteLine("WaitForEnd begin");
        if (m_IsStarted)
        {
            m_Thread.Join();
        }
        Console.WriteLine("WaitForEnd end");
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


