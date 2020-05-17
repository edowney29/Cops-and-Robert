using System;
using Dissonance.Networking;

public class CustomServer : BaseServer<CustomServer, CustomClient, CustomConn>
{
    NetworkManager networkManager;

    public CustomServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;
    }

    protected override void ReadMessages()
    {
        networkManager.voiceHolderServer.ForEach(voice =>
          {
              CustomConn conn = new CustomConn()
              {
                  token = voice.Token
              };
              NetworkReceivedPacket(conn, new ArraySegment<byte>(voice.Data));
          });
        networkManager.voiceHolderServer.Clear();
    }

    protected override void SendReliable(CustomConn conn, ArraySegment<byte> packet)
    {
        SendPacket(conn, packet.Array);
    }

    protected override void SendUnreliable(CustomConn conn, ArraySegment<byte> packet)
    {
        SendPacket(conn, packet.Array);
    }

    void SendPacket(CustomConn conn, byte[] data)
    {
        var packet = new VoicePacket
        {
            Dest = conn.token,
            IsP2P = false,
            Data = data,
        };

        // if (conn.token.Equals(networkManager.Token))
        // {
        //     packet.Token = conn.token;
        //     networkManager.voiceHolderClient.Add(packet);
        // }
        // else
        // {
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(packet);
        networkManager.WebSocket.SendText(json);
        // }
    }
}
