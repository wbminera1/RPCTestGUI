using System;
using System.IO;


public class RPCObject
{
    const UInt32 Signature = 0x1234567;
    enum ObjectType
    {
        oFirst,
        oRequest,
        oResponse,
        oLast
    }

    ObjectType  m_ObjectType;
    Int32       m_ObjectDataSize;
    byte[]      m_ObjectData;

    RPCObject()
    {

    }
    RPCObject(ObjectType type)
    {
        m_ObjectType = type;
    }
    public bool Deserialize(BinaryReader data)
    {
        m_ObjectType = (ObjectType)data.ReadInt16();
        m_ObjectDataSize = data.ReadInt32();
        m_ObjectData = data.ReadBytes(m_ObjectDataSize);
        return m_ObjectData.Length == m_ObjectDataSize;
    }
    public void Serialize(BinaryWriter writer)
    {
        writer.Write((Int16)m_ObjectType);
        writer.Write(m_ObjectDataSize);
        writer.Write(m_ObjectData);
    }
    public int Size()
    {
        return sizeof(Int16) + sizeof(Int32) + m_ObjectData.Length;
    }
    public static RPCObject Create(byte[] data)
    {
        if(data.Length >= (sizeof(Int16) + sizeof(Int32)))
        {
            RPCObject rpcobject = new RPCObject();
            if(rpcobject.Deserialize(new BinaryReader(new MemoryStream(data))))
            {
                return rpcobject;
            }
        }
        return null;
    }
    public static void Test()
    {
        RPCObject testObj = new RPCObject(ObjectType.oRequest);
        BinaryWriter writer = new BinaryWriter(new MemoryStream(1024));
        testObj.Serialize(writer);
        testObj.Deserialize(new BinaryReader(writer.BaseStream));
    }
}

public class RPCCommand
{
    static protected UInt32 RPCVersion = 0x0001;

    protected enum RequestType
    {
        cFirst,
        //cError,
        //cResponse,
        cConnect,
        cDisconnect,
        cExit,
        cJSONCommand,
        cBinaryCommand,
        cLast
    };

    private RequestType m_Type;
    private UInt32 m_Id;

    protected RPCCommand(RequestType type, UInt32 id)
    {
        m_Type = type;
        m_Id = id;
    }

    /*
            virtual public byte[] Serialize()
            {
                byte[] typeBytes = BitConverter.GetBytes((Int32)m_Type);
                byte[] idBytes = BitConverter.GetBytes(m_Id);
                byte[] dataBytes = new byte[typeBytes.Length + idBytes.Length];
                System.Buffer.BlockCopy(typeBytes, 0, dataBytes, 0, typeBytes.Length);
                System.Buffer.BlockCopy(idBytes, 0, dataBytes, typeBytes.Length, idBytes.Length);
                return dataBytes;
            }
    */

    virtual public void Serialize(BinaryWriter writer)
    {
        writer.Write((Int32)m_Type);
        writer.Write(m_Id);
    }

    virtual public int Deserialize(byte[] data)
    {
        m_Type = (RequestType)BitConverter.ToInt32(data, 0);
        m_Id = BitConverter.ToUInt32(data, sizeof(Int32));
        return sizeof(Int32) + sizeof(UInt32);
    }

    public static void Test()
    {
        RPCCommand cmd = new RPCCommand(RequestType.cFirst, 0x1234);
        /*
                    byte[] data = cmd.Serialize();
                    cmd.Deserialize(data);
        */
    }
}

public class RPCResponse
{
    protected enum ResponseType
    {
        rFirst,
        rOk,
        rError,
        rLast
    };

}

public class RPCCommandConnect : RPCCommand
{

    private UInt32 m_Version;

    public RPCCommandConnect(UInt32 id) : base(RequestType.cConnect, id)
    {
        m_Version = RPCVersion;
    }

    public override int Deserialize(byte[] data)
    {
        int offset = base.Deserialize(data);
        m_Version = BitConverter.ToUInt32(data, offset);
        return offset + sizeof(UInt32);
    }

    /*
        public override byte[] Serialize()
        {
            byte[] baseBytes = base.Serialize(); ;
            byte[] versionBytes = BitConverter.GetBytes(m_Version);
            byte[] dataBytes = new byte[baseBytes.Length + versionBytes.Length];
            System.Buffer.BlockCopy(baseBytes, 0, dataBytes, 0, baseBytes.Length);
            System.Buffer.BlockCopy(versionBytes, 0, dataBytes, baseBytes.Length, versionBytes.Length);
            return dataBytes;
        }
    */

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(m_Version);
    }

}

public interface ICommandHandler
{
    bool Receive(RPCCommand cmd);
    bool Send(RPCCommand cmd);
}

class CommandDispatcher
{

}

class ClientCommandHandler : Client, ICommandHandler
{
    //    private FrontEnd.DebugConsole m_DebugConsole;

    public ClientCommandHandler(/*FrontEnd.DebugConsole debugConsole*/) : base(/*debugConsole*/)
    {
        //m_DebugConsole = debugConsole;
        Connect("127.0.0.1", 9999);
    }

    public bool Receive(RPCCommand cmd)
    {
        throw new NotImplementedException();
    }

    public bool Send(RPCCommand cmd)
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);
        cmd.Serialize(writer);
        return SendWithSize(stream);
    }
}