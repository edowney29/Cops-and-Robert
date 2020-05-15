using Dissonance.Networking;
using System;
using System.Collections.Generic;

public class CustomClient : BaseClient<CustomServer, CustomClient, CustomConn>
{
    private readonly NetworkController networkController;
    CustomConn conn = new CustomConn();

    public CustomClient(CustomCommsNetwork network, NetworkController networkController) : base(network)
    {
        this.networkController = networkController;
    }

    public override void Connect()
    {
        if (networkController.WebSocket.State == NativeWebSocket.WebSocketState.Open)
        {
            base.Connected();
        }
    }

    protected override void ReadMessages()
    {
        networkController.voiceHolderClient.ForEach(voice =>
        {
            var id = base.NetworkReceivedPacket(new ArraySegment<byte>(voice.Data));
            if (id.HasValue)
            {
                // CustomConn _conn = new CustomConn()
                // {
                //     token = voice.Token
                // };
                conn.token = voice.Token;
                ReceiveHandshakeP2P(id.Value, conn);
            }
        });
        networkController.voiceHolderClient.Clear();
    }

    protected override void SendReliable(ArraySegment<byte> packet)
    {
        SendPacket(packet.Array, false);
    }

    protected override void SendUnreliable(ArraySegment<byte> packet)
    {
        SendPacket(packet.Array, false);
    }

    private void SendReliableP2P(IList<ClientInfo<CustomConn?>> destinations, ArraySegment<byte> packet)
    {
        SendPacket(packet.Array, true);
        destinations.Clear();
        base.SendReliableP2P((List<ClientInfo<CustomConn?>>)destinations, packet);
    }

    private void SendUnreliableP2P(IList<ClientInfo<CustomConn?>> destinations, ArraySegment<byte> packet)
    {
        SendPacket(packet.Array, true);
        destinations.Clear();
        base.SendUnreliableP2P((List<ClientInfo<CustomConn?>>)destinations, packet);
    }

    protected override void OnServerAssignedSessionId(uint session, ushort id)
    {
        base.OnServerAssignedSessionId(session, id);
        var packet = new ArraySegment<byte>(WriteHandshakeP2P(session, id));
        SendPacket(packet.Array, true);
    }

    public override void Disconnect()
    {
        base.Disconnect();
    }

    void SendPacket(byte[] data, bool isP2P)
    {
        var packet = new VoicePacket
        {
            Dest = isP2P ? "" : networkController.ServerToken,
            IsP2P = isP2P,
            Data = data,
        };

        // For when client is also server loopback
        if (networkController.IsServer && !networkController.comms.IsNetworkInitialized)
        {
            packet.Token = networkController.Token;
            networkController.voiceHolderServer.Add(packet);
        }
        else
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(packet);
            networkController.WebSocket.SendText(json);
        }
    }
}
