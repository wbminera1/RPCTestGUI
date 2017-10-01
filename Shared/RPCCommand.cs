using Shared;
using System;
using System.Collections.Generic;
using System.IO;


public class RPCObject
{
    const UInt32 Signature = 0x1234567;
    public enum ObjectType
    {
        oFirst,
        oRequest,
        oResponse,
        oLast
    }

    ObjectType  m_ObjectType;
    Int32       m_ObjectDataSize;
    byte[]      m_ObjectData;

    public RPCObject()
    {
        m_ObjectData = new byte[0];
    }
    public RPCObject(ObjectType type) : this()
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
        var memStream = new MemoryStream(1024);
        BinaryWriter writer = new BinaryWriter(memStream);
        testObj.Serialize(writer);
        memStream.Seek(0, SeekOrigin.Begin);
        testObj.Deserialize(new BinaryReader(memStream));
    }
}

public class RPCRequest
{
    static protected UInt32 RPCVersion = 0x0001;

    public enum RequestType
    {
        cFirst,
        cConnect,
        cDisconnect,
        cExit,
        cJSONCommand,
        cBinaryCommand,
        cLast
    };

    public RequestType Type { get; set; }
    public UInt32 Id { get; set; }

    public RPCRequest()
    {
        Type = RequestType.cFirst;
        Id = 0;
    }
    protected RPCRequest(RequestType type, UInt32 id)
    {
        Type = type;
        Id = id;
    }
    virtual public void Serialize(BinaryWriter writer)
    {
        writer.Write((Int16)Type);
        writer.Write(Id);
    }
    virtual public bool Deserialize(BinaryReader reader)
    {
        bool result = false;
        try
        {
            Type = (RequestType)reader.ReadInt16();
            Id = reader.ReadUInt32();
            result = true;
        }
        catch(Exception)
        { }
        return result;
    }
    public static RPCRequest Create(byte[] data)
    {
        if (data.Length >= (sizeof(Int16) + sizeof(UInt32)))
        {
            RPCRequest rpcrequest = new RPCRequest();
            if (rpcrequest.Deserialize(new BinaryReader(new MemoryStream(data))))
            {
                return rpcrequest;
            }
        }
        return null;
    }
    public static void Test()
    {
        RPCRequest req = new RPCRequest(RequestType.cFirst, 0x1234);
        var memStream = new MemoryStream(1024);
        BinaryWriter writer = new BinaryWriter(memStream);
        req.Serialize(writer);
        memStream.Seek(0, SeekOrigin.Begin);
        req.Deserialize(new BinaryReader(memStream));

        RPCRequest creq = Singleton<RPCRequestFactory>.Instance.Create(RPCRequest.RequestType.cConnect);
    }

}

public class RPCRequestFactory : GenericFactory<RPCRequest.RequestType>
{
    public RPCRequestFactory()
    {
        Register<RPCRequest>(RPCRequest.RequestType.cFirst);
        Register<RPCRequestConnect>(RPCRequest.RequestType.cConnect);
    }
    public RPCRequest Create(RPCRequest.RequestType reqType, UInt32 id = 0)
    {
        RPCRequest req = base.Create<RPCRequest>(reqType);
        req.Id = id;
        return req;
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

public class RPCRequestConnect : RPCRequest
{

    private UInt32 m_Version;

    public RPCRequestConnect() : base(RequestType.cConnect, 0)
    {

    }
    public RPCRequestConnect(UInt32 id) : base(RequestType.cConnect, id)
    {
        m_Version = RPCVersion;
    }

/*
    public override int Deserialize(byte[] data)
    {
        int offset = base.Deserialize(data);
        m_Version = BitConverter.ToUInt32(data, offset);
        return offset + sizeof(UInt32);
    }
*/

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
    bool Receive(RPCRequest cmd);
    bool Send(RPCRequest cmd);
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

    public bool Receive(RPCRequest cmd)
    {
        throw new NotImplementedException();
    }

    public bool Send(RPCRequest cmd)
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);
        cmd.Serialize(writer);
        return SendWithSize(stream);
    }
}