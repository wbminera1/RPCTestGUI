using System;
using System.IO;

public class RPCCommand
    {
        protected enum CommandType {
            cFirst,
            cError,
            cResponse,
            cConnect,
            cDisconnect,
            cExit,
            cJSONCommand,
            cBinaryCommand,
            cLast
        };

        private CommandType m_Type;
        private UInt32 m_Id;

        protected RPCCommand(CommandType type, UInt32 id)
        {
            m_Type = type;
            m_Id = id;
        }

        virtual public byte[] Serialize()
        {
            byte[] typeBytes = BitConverter.GetBytes((Int32)m_Type);
            byte[] idBytes = BitConverter.GetBytes(m_Id);
            byte[] dataBytes = new byte[typeBytes.Length + idBytes.Length];
            System.Buffer.BlockCopy(typeBytes, 0, dataBytes, 0, typeBytes.Length);
            System.Buffer.BlockCopy(idBytes, 0, dataBytes, typeBytes.Length, idBytes.Length);
            return dataBytes;
        }

        virtual public void Serialize(BinaryWriter writer)
        {
            writer.Write((Int32)m_Type);
            writer.Write(m_Id);
        }

        virtual public int Deserialize(byte[] data)
        {
            m_Type = (CommandType)BitConverter.ToInt32(data, 0);
            m_Id = BitConverter.ToUInt32(data, sizeof(Int32));
            return sizeof(Int32) + sizeof(UInt32);
        }

        public static void Test()
        {
            RPCCommand cmd = new RPCCommand(CommandType.cFirst, 0x1234);
            byte[] data = cmd.Serialize();
            cmd.Deserialize(data);
        }
    }


public class RPCCommandConnect : RPCCommand
{

    static private UInt32 RPCVersion = 0x0001;
    private UInt32 m_Version;

    public RPCCommandConnect(UInt32 id) : base(CommandType.cConnect, id)
    {
        m_Version = RPCVersion;
    }

    public override int Deserialize(byte[] data) 
    {
        int offset = base.Deserialize(data);
        m_Version = BitConverter.ToUInt32(data, offset);
        return offset + sizeof(UInt32);
    }

    public override byte[] Serialize()
    {
        byte[] baseBytes = base.Serialize(); ;
        byte[] versionBytes = BitConverter.GetBytes(m_Version);
        byte[] dataBytes = new byte[baseBytes.Length + versionBytes.Length];
        System.Buffer.BlockCopy(baseBytes, 0, dataBytes, 0, baseBytes.Length);
        System.Buffer.BlockCopy(versionBytes, 0, dataBytes, baseBytes.Length, versionBytes.Length);
        return dataBytes;
    }

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