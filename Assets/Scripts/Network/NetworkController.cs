using NativeWebSocket;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class NetworkController : MonoBehaviour
{
    [SerializeField]
    GameObject prefabPlayer, prefabOtherPlayer, menuPanel, locationPanel;
    [SerializeField]
    TMP_InputField usernameInput, passwordInput;
    [SerializeField]
    TMP_Text locationText;

    GameObject player;
    Dissonance.DissonanceComms comms;

    PlayerJSON playerJson = new PlayerJSON();
    Dictionary<string, OtherController> otherPlayers = new Dictionary<string, OtherController>();

    public string token, username, roomId;
    public bool isServer = false;
    public List<VoicePacket> voiceHolderClient = new List<VoicePacket>();
    public List<VoicePacket> voiceHolderServer = new List<VoicePacket>();

    public WebSocket WebSocket { get; private set; }

    void Start()
    {
        comms = GetComponent<Dissonance.DissonanceComms>();

        usernameInput.onValueChanged.AddListener(SetUsername);
        passwordInput.onValueChanged.AddListener(SetRoomId);

        //InvokeRepeating("SendPlayerJSON", 1f, 0.33333333f);
        StartCoroutine("SendPlayerJSON", 0.33333333f);
    }

    async void StartWebSocket()
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
            roomId = null;
            comms.enabled = false;
            locationPanel.SetActive(false);
            menuPanel.SetActive(true);
            Destroy(player);
            Destroy(Camera.main.gameObject);
        };

        WebSocket.OnMessage += (bytes) =>
        {
            string json = Encoding.UTF8.GetString(bytes);
            Debug.Log(json);
            // foreach (string json in jsonHolder.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("}{", "}|{").Split('|'))
            // foreach (string json in Regex.Replace(jsonHolder, "/(\r\n)|\n|\r/gm", "|").Split('|'))
            // foreach (string json in jsonHolder.Split('|'))
            // {

            if (json.Equals("isServer"))
            {
                isServer = true;
            }
            else if (json.Contains("Username"))
            {
                PlayerPacket packet = JsonConvert.DeserializeObject<PlayerPacket>(json);
                if (packet.Token.Equals(token)) return;
                if (otherPlayers.TryGetValue(packet.Token, out OtherController oc))
                {
                    oc.UpdateTransform(packet);
                }
                else
                {
                    GameObject obj = Instantiate(prefabOtherPlayer, new Vector3(packet.X, packet.Y, packet.Z), Quaternion.Euler(packet.RX, packet.RY, packet.RZ));
                    // obj.name = packet.Token;
                    obj.GetComponent<VoiceController>().StartVoice(packet.Token);
                    // obj.GetComponent<Dissonance.VoiceBroadcastTrigger>().PlayerId = packet.Token;
                    otherPlayers.Add(packet.Token, obj.GetComponent<OtherController>());
                }
            }
            else
            {
                VoicePacket packet = JsonConvert.DeserializeObject<VoicePacket>(json);
                if (packet.Conn.id.Equals(token) && !isServer) return; // As server from my own client can pass
                if (isServer && !packet.IsServer && !packet.IsP2P)
                {
                    Debug.Log("SERVER: " + packet.Conn.id + " - " + packet.IsServer + " - " + packet.IsP2P);
                    voiceHolderServer.Add(packet);
                }
                else if (packet.IsServer || packet.IsP2P)
                {
                    Debug.Log("CLIENT: " + packet.Conn.id + " - " + packet.IsServer + " - " + packet.IsP2P);
                    voiceHolderClient.Add(packet);
                }
                else
                {
                    Debug.Log("Unknown voice packet");
                }
            }
            // }
        };

        await WebSocket.Connect();
    }

    public void SpawnPlayer(bool isServer)
    {
        StartWebSocket();

        // this.isServer = isServer;
        token = Guid.NewGuid().ToString();
        comms.LocalPlayerName = token;
        comms.enabled = true;
        player = Instantiate(prefabPlayer);
        // player.name = token;
        player.GetComponent<VoiceController>().StartVoice(token);
        locationPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    IEnumerator SendPlayerJSON(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            if (WebSocket != null && player != null && token != null && roomId != null)
            {
                if (WebSocket.State == WebSocketState.Open)
                {
                    playerJson.Token = token;
                    playerJson.Username = username;
                    playerJson.X = player.transform.position.x;
                    playerJson.Y = player.transform.position.y;
                    playerJson.Z = player.transform.position.z;
                    playerJson.RX = player.transform.rotation.eulerAngles.x;
                    playerJson.RY = player.transform.rotation.eulerAngles.y;
                    playerJson.RZ = player.transform.rotation.eulerAngles.z;
                    string json = JsonConvert.SerializeObject(playerJson);
                    WebSocket.SendText(json);
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        // WebSocket.Close();
    }

    public void ToggleMic(bool toggle)
    {
        // enableMic = toggle;
    }

    public void SetUsername(string username)
    {
        this.username = username;
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

struct PlayerJSON
{
    public string Token { get; set; }
    public string Username { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float RX { get; set; }
    public float RY { get; set; }
    public float RZ { get; set; }
}

public class PlayerPacket
{
    public string Token { get; set; }
    public string Username { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float RX { get; set; }
    public float RY { get; set; }
    public float RZ { get; set; }
}

public class VoicePacket
{
    public byte[] Data { get; set; }
    public bool IsServer { get; set; }
    public bool IsP2P { get; set; }
    public CustomConn Conn { get; set; }
}