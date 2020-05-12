using System;
using Dissonance.Networking;

public class CustomServer : BaseServer<CustomServer, CustomClient, CustomConn>
{
    CustomCommsNetwork network;
    NetworkController networkController;

    public CustomServer(CustomCommsNetwork network, NetworkController networkController)
    {
        this.network = network;
        this.networkController = networkController;
    }

    protected override void ReadMessages()
    {
        networkController.voiceHolderServer.ForEach(voice =>
          {
              base.NetworkReceivedPacket(voice.Conn, new ArraySegment<byte>(voice.Data));
          });
        networkController.voiceHolderServer.Clear();
    }

    protected override void SendReliable(CustomConn connection, ArraySegment<byte> packet)
    {
        var obj = new VoicePacket
        {
            Conn = connection,
            IsServer = true,
            IsP2P = false,
            Data = packet.Array
        };
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        networkController.WebSocket.SendText(json);
    }

    protected override void SendUnreliable(CustomConn connection, ArraySegment<byte> packet)
    {
        var obj = new VoicePacket
        {
            Conn = connection,
            IsServer = true,
            IsP2P = false,
            Data = packet.Array
        };
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        networkController.WebSocket.SendText(json);
    }
}
