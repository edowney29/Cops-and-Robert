using System;
using Dissonance.Networking;

public class CustomServer : BaseServer<CustomServer, CustomClient, CustomConn>
{
    // CustomCommsNetwork network;
    NetworkController networkController;

    public CustomServer(NetworkController networkController)
    {
        // this.network = network;
        this.networkController = networkController;
    }

    protected override void ReadMessages()
    {
        networkController.voiceHolderServer.ForEach(voice =>
          {
              NetworkReceivedPacket(voice.Conn, new ArraySegment<byte>(voice.Data));
          });
        networkController.voiceHolderServer.Clear();
    }

    protected override void SendReliable(CustomConn conn, ArraySegment<byte> packet)
    {
        var obj = new VoicePacket
        {
            Conn = conn,
            IsServer = true,
            IsP2P = false,
            Data = packet.Array
        };

        // if (conn.id.Equals(networkController.token))
        // {
        //     networkController.voiceHolderServer.Add(obj);
        // }
        // else
        // {
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        networkController.WebSocket.SendText(json);
        // }
    }

    protected override void SendUnreliable(CustomConn conn, ArraySegment<byte> packet)
    {
        var obj = new VoicePacket
        {
            Conn = conn,
            IsServer = true,
            IsP2P = false,
            Data = packet.Array
        };

        // if (conn.id.Equals(networkController.token))
        // {
        //     networkController.voiceHolderServer.Add(obj);
        // }
        // else
        // {
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        networkController.WebSocket.SendText(json);
        // }
    }
}
