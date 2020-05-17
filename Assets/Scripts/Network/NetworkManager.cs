using NativeWebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    [SerializeField]
    GameObject prefabPlayer, prefabOtherPlayer, menuPanel, locationPanel;
    [SerializeField]
    TMP_InputField usernameInput, passwordInput;
    [SerializeField]
    TMP_Text locationText;

    string roomId;
    GameObject player;
    PlayerJson playerJson = new PlayerJson();
    Dictionary<string, OtherController> otherPlayers = new Dictionary<string, OtherController>();

    public List<PlayerPacket> voiceHolderClient = new List<PlayerPacket>();
    public List<PlayerPacket> voiceHolderServer = new List<PlayerPacket>();

    public Dissonance.DissonanceComms comms { get; private set; }
    public WebSocket WebSocket { get; private set; }
    public string Token { get; private set; }
    public bool IsServer { get; private set; }
    public string ServerToken { get; private set; }

    void Start()
    {
        comms = GetComponent<Dissonance.DissonanceComms>();

        usernameInput.onValueChanged.AddListener(SetUsername);
        passwordInput.onValueChanged.AddListener(SetRoomId);

        locationPanel.SetActive(false);

        InvokeRepeating("SendPlayerJSON", 0f, 0.33333333f);
    }

    public async void StartWebSocket()
    {
        WebSocket = new WebSocket("ws://cops-and-robert-server.herokuapp.com/ws/" + roomId);

        WebSocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        WebSocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        WebSocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            Token = null;
            comms.enabled = false;
            locationPanel.SetActive(false);
            menuPanel.SetActive(true);
            Destroy(player);
            Destroy(Camera.main.gameObject);
        };

        WebSocket.OnMessage += (bytes) =>
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            // foreach (string json in jsonHolder.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("}{", "}|{").Split('|'))
            // foreach (string json in Regex.Replace(jsonHolder, "/(\r\n)|\n|\r/gm", "|").Split('|'))
            // foreach (string json in jsonHolder.Split('|'))
            // {          
            PlayerPacket packet = JsonConvert.DeserializeObject<PlayerPacket>(json);
            if (packet.IsServer) ServerToken = packet.Token;
            Debug.Log("[PACKET]: TYPE_" + packet.Type + " --- " + packet.Token + " --- " + packet.IsServer);

            if (Token == null)
            {
                Token = packet.Token;
                IsServer = packet.IsServer;
                // username = packet.Username;
                SpawnPlayer();
            }
            else if (packet.Type == PacketType.Player)
            {
                if (otherPlayers.TryGetValue(packet.Token, out OtherController oc))
                {
                    oc.UpdateTransform(packet);
                }
                else
                {
                    GameObject obj = Instantiate(prefabOtherPlayer, new Vector3(packet.PosX, packet.PosY, packet.PosZ), Quaternion.Euler(packet.RotX, packet.RotY, packet.RotZ));
                    // obj.name = packet.Token;
                    obj.GetComponent<VoiceController>().StartVoice(packet.Token);
                    otherPlayers.Add(packet.Token, obj.GetComponent<OtherController>());
                }
            }
            else if (packet.Type == PacketType.Voice)
            {
                if (IsServer && !packet.IsServer && !packet.IsP2P)
                {
                    // Debug.Log("SERVER: " + packet.Token + " - " + packet.IsServer + " - " + packet.IsP2P + " - " + comms.IsNetworkInitialized);
                    voiceHolderServer.Add(packet);
                }
                else if (packet.IsServer || packet.IsP2P)
                {
                    // Debug.Log("CLIENT: " + packet.Token + " - " + packet.IsServer + " - " + packet.IsP2P + " - " + comms.IsNetworkInitialized);
                    voiceHolderClient.Add(packet);
                }
                else
                {

                }
            }
            else if (packet.Type == PacketType.Game)
            {
            }
            else
            {

            }
        };

        await WebSocket.Connect();
    }

    public void SpawnPlayer()
    {
        comms.LocalPlayerName = Token;
        comms.enabled = true;
        player = Instantiate(prefabPlayer);
        // player.name = Token;
        player.GetComponent<VoiceController>().StartVoice(Token);
        locationPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    async void SendPlayerJSON()
    {
        if (WebSocket != null && player != null && Token != null)
        {
            playerJson.PosX = player.transform.position.x;
            playerJson.PosY = player.transform.position.y;
            playerJson.PosZ = player.transform.position.z;
            playerJson.RotX = player.transform.rotation.eulerAngles.x;
            playerJson.RotY = player.transform.rotation.eulerAngles.y;
            playerJson.RotZ = player.transform.rotation.eulerAngles.z;
            if (WebSocket.State == WebSocketState.Open)
            {
                string json = JsonConvert.SerializeObject(playerJson);
                await WebSocket.SendText(json);
            }
        }
    }

    async void SendGameJSON()
    {
        if (WebSocket != null)
        {

            if (WebSocket.State == WebSocketState.Open)
            {
                string json = JsonConvert.SerializeObject(playerJson);
                await WebSocket.SendText(json);
            }
        }
    }

    public void ToggleMic(bool toggle)
    {
        // enableMic = toggle;
    }

    public void SetUsername(string username)
    {
        // this.username = username;
    }

    public void SetRoomId(string roomId)
    {
        this.roomId = roomId;
    }

    public void SetLocation(string location)
    {
        locationText.SetText(location);
    }
}

public enum PacketType
{
    Player,
    Voice,
    Game,
}

public class PlayerJson
{
    public readonly PacketType Type;
    public float PosX, PosY, PosZ, RotX, RotY, RotZ;

    public PlayerJson()
    {
        Type = PacketType.Player;
    }

}

public class VoiceJson
{
    public readonly PacketType Type;
    public readonly string Dest;
    public readonly byte[] Data;
    public readonly bool IsP2P;

    public VoiceJson(string dest, byte[] data, bool isP2P)
    {
        Type = PacketType.Voice;
        Dest = dest;
        Data = data;
        IsP2P = isP2P;
    }
}

public class PlayerPacket
{
    public readonly PacketType Type;
    public readonly string Token, Username, Dest;
    public readonly bool IsServer, IsP2P;
    public readonly float PosX, PosY, PosZ, RotX, RotY, RotZ;
    public readonly byte[] Data;
    // public string GameState;

    public PlayerPacket(string token, byte[] data)
    {
        Token = token;
        Data = data;
    }
}
