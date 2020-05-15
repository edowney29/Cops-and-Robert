using Dissonance.Networking;
using System;
using System.Collections.Generic;

public class CustomClient : BaseClient<CustomServer, CustomClient, CustomConn>
{
    private readonly NetworkController networkController;
    private CustomConn conn = new CustomConn();

    public CustomClient(CustomCommsNetwork network, NetworkController networkController) : base(network)
    {
        this.networkController = networkController;
    }

    public override void Connect()
    {
        if (networkController.WebSocket.State == NativeWebSocket.WebSocketState.Open && networkController.Token != null)
        {
            conn.token = networkController.Token;
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
                CustomConn _conn = new CustomConn()
                {
                    token = voice.Token
                };
                ReceiveHandshakeP2P(id.Value, _conn);
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
        var obj = new VoicePacket
        {
            IsP2P = isP2P,
            Data = data,
        };

        if (conn.token.Equals(networkController.Token))
        {
            obj.Token = conn.token;
            networkController.voiceHolderServer.Add(obj);
        }
        else
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            networkController.WebSocket.SendText(json);
        }
    }
}
