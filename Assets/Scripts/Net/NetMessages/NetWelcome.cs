using UnityEngine;
using Unity.Networking.Transport;

public class NetWelcome : NetMessage
{
    public int AssignedTeam { set; get; }
    public NetWelcome() // making the box
    {
        Code = OpCode.WELCOME;
    }
    public NetWelcome(DataStreamReader reader) //receive the box
    {
        Code = OpCode.WELCOME;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(AssignedTeam);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        //already read the byte in the NetUtility :: OnData
        AssignedTeam = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_WELCOME?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_WELCOME?.Invoke(this, cnn);
    }
}
