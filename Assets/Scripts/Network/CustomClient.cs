using Dissonance.Networking;
using System;
using System.Collections.Generic;

public class CustomClient : BaseClient<CustomServer, CustomClient, CustomConn>
{
    private readonly NetworkManager networkManager;
    CustomConn conn = new CustomConn();

    public CustomClient(CustomCommsNetwork network, NetworkManager networkManager) : base(network)
    {
        this.networkManager = networkManager;
    }

    public override void Connect()
    {
        base.Connected();
    }

    protected override void ReadMessages()
    {
        networkManager.voiceHolderClient.ForEach(voice =>
        {
            var id = base.NetworkReceivedPacket(new ArraySegment<byte>(voice.Data));
            if (id.HasValue)
            {
                // CustomConn _conn = new CustomConn()
                // {
                //     token = voice.Token
                // };
                // conn.token = voice.Token;
                // ReceiveHandshakeP2P(id.Value, conn);
            }
        });
        networkManager.voiceHolderClient.Clear();
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
        // var packet = new ArraySegment<byte>(WriteHandshakeP2P(session, id));
        // SendPacket(packet.Array, true);
    }

    public override void Disconnect()
    {
        base.Disconnect();
    }

    async void SendPacket(byte[] data, bool isP2P)
    {
        // For when client is also server loopback
        if (networkManager.IsServer)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(new VoiceJson(networkManager.ServerToken, data, isP2P));
            PlayerPacket packet = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerPacket>(json);
            networkManager.voiceHolderServer.Add(packet);
        }
        else
        {
            var packet = new VoiceJson(isP2P ? "" : networkManager.ServerToken, data, isP2P);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(packet);
            await networkManager.WebSocket.SendText(json);
        }
    }
}
