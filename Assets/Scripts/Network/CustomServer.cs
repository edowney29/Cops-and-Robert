﻿using System;
using Dissonance.Networking;

public class CustomServer : BaseServer<CustomServer, CustomClient, CustomConn>
{
    NetworkController networkController;

    public CustomServer(NetworkController networkController)
    {
        this.networkController = networkController;
    }

    protected override void ReadMessages()
    {
        networkController.voiceHolderServer.ForEach(voice =>
          {
              CustomConn conn = new CustomConn()
              {
                  token = voice.Token
              };
              NetworkReceivedPacket(conn, new ArraySegment<byte>(voice.Data));
          });
        networkController.voiceHolderServer.Clear();
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

        // if (conn.token.Equals(networkController.Token))
        // {
        //     packet.Token = conn.token;
        //     networkController.voiceHolderClient.Add(packet);
        // }
        // else
        // {
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(packet);
        networkController.WebSocket.SendText(json);
        // }
    }
}
