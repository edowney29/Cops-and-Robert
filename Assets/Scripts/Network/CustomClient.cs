using Dissonance.Networking;
using System;
using System.Collections.Generic;

public class CustomClient : BaseClient<CustomServer, CustomClient, CustomConn>
{
    private readonly NetworkController networkController;
    private CustomConn connection = new CustomConn();

    public CustomClient(CustomCommsNetwork network, NetworkController networkController) : base(network)
    {
        this.networkController = networkController;
    }

    public override void Connect()
    {
        if (networkController.WebSocket.State == NativeWebSocket.WebSocketState.Open && networkController.token != null)
        {
            connection.id = networkController.token;
            base.Connected();
        }
    }

    protected override void ReadMessages()
    {
        networkController.voiceHolderClient.ForEach(voice =>
        {
            var id = base.NetworkReceivedPacket(new ArraySegment<byte>(voice.Data));
            // If the value is not null
            // pass to handshake method with the `senderid` of this packet
            if (id.HasValue)
                ReceiveHandshakeP2P(id.Value, voice.Conn);
        });
        networkController.voiceHolderClient.Clear();
    }

    protected override void SendReliable(ArraySegment<byte> packet)
    {
        SendPacket(connection, packet.Array, false, false);
    }

    protected override void SendUnreliable(ArraySegment<byte> packet)
    {
        SendPacket(connection, packet.Array, false, false);
    }

    private void SendReliableP2P(IList<ClientInfo<CustomConn?>> destinations, ArraySegment<byte> packet)
    {
        destinations.Clear();
        SendPacket(connection, packet.Array, false, true);
        base.SendReliableP2P((List<ClientInfo<CustomConn?>>)destinations, packet);
    }

    private void SendUnreliableP2P(IList<ClientInfo<CustomConn?>> destinations, ArraySegment<byte> packet)
    {
        // Build a list of destinations we know how to send to
        // i.e. have a non-null Connection object
        // var dests = new List<CustomConn>();
        // foreach (var item in destinations)
        //     if (item.Connection.HasValue)
        //         dests.Add(item.Connection);

        // Remove all the ones we can send to from the input list
        // destinations.RemoveAll(dests);
        destinations.Clear();

        // Send the packets to the list of destinations through PUN
        // _network.Send(packet, dests, reliable: false);
        SendPacket(connection, packet.Array, false, true);

        // Call base to do server relay for all the peers we don't
        // know how to contact
        base.SendUnreliableP2P((List<ClientInfo<CustomConn?>>)destinations, packet);
    }

    protected override void OnServerAssignedSessionId(uint session, ushort id)
    {
        base.OnServerAssignedSessionId(session, id);

        // Create the handshake packet to send
        var packet = new ArraySegment<byte>(WriteHandshakeP2P(session, id));
        SendPacket(connection, packet.Array, false, true);

        // Send this to everyone else in the session through PUN
        // _network.Send(packet, _network.EventCodeToClient, new RaiseEventOptions {
        //     Receivers = ReceiverGroup.Others,
        // }, true);
    }

    public override void Disconnect()
    {
        base.Disconnect();
    }

    void SendPacket(CustomConn conn, byte[] data, bool isServer, bool isP2P)
    {
        var obj = new VoicePacket
        {
            Conn = conn,
            IsServer = isServer,
            IsP2P = isP2P,
            Data = data,
        };
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        networkController.WebSocket.SendText(json);
    }
}
