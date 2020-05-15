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

    string roomId;
    GameObject player;
    PlayerJSON playerJson = new PlayerJSON();
    Dictionary<string, OtherController> otherPlayers = new Dictionary<string, OtherController>();
    public List<VoicePacket> voiceHolderClient = new List<VoicePacket>();
    public List<VoicePacket> voiceHolderServer = new List<VoicePacket>();

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
        // StartCoroutine("SendPlayerJSON", 0.33333333f);
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
            // Debug.Log(json);
            // foreach (string json in jsonHolder.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("}{", "}|{").Split('|'))
            // foreach (string json in Regex.Replace(jsonHolder, "/(\r\n)|\n|\r/gm", "|").Split('|'))
            // foreach (string json in jsonHolder.Split('|'))
            // {
            if (!json.Contains("Data"))
            {
                PlayerPacket packet = JsonConvert.DeserializeObject<PlayerPacket>(json);
                if (packet.Token.Equals(Token)) return;
                if (Token == null)
                {
                    Token = packet.Token;
                    IsServer = packet.IsServer;
                    // username = packet.Username;
                    SpawnPlayer();
                }
                else
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
            }
            else
            {
                VoicePacket packet = JsonConvert.DeserializeObject<VoicePacket>(json);
                if (packet.IsServer) ServerToken = packet.Token;
                if (IsServer && !packet.IsServer && !packet.IsP2P)
                {
                    Debug.Log("SERVER: " + packet.Token + " - " + packet.IsServer + " - " + packet.IsP2P + " - " + comms.IsNetworkInitialized);
                    voiceHolderServer.Add(packet);
                }
                else if (packet.IsServer || packet.IsP2P)
                {
                    Debug.Log("CLIENT: " + packet.Token + " - " + packet.IsServer + " - " + packet.IsP2P + " - " + comms.IsNetworkInitialized);
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
            // playerJson.Token = token;
            // playerJson.Username = username;
            // playerJson.IsServer = isServer;
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

    // IEnumerator SendPlayerJSON(float waitTime)
    // {
    //     while (true)
    //     {
    //         yield return new WaitForSeconds(waitTime);
    //         if (WebSocket != null && player != null && token != null && roomId != null)
    //         {
    //             if (WebSocket.State == WebSocketState.Open)
    //             {
    //                 playerJson.Token = token;
    //                 playerJson.Username = username;
    //                 playerJson.IsServer = isServer;
    //                 playerJson.PosX = player.transform.position.x;
    //                 playerJson.PosY = player.transform.position.y;
    //                 playerJson.PosZ = player.transform.position.z;
    //                 playerJson.RotX = player.transform.rotation.eulerAngles.x;
    //                 playerJson.RotY = player.transform.rotation.eulerAngles.y;
    //                 playerJson.RotZ = player.transform.rotation.eulerAngles.z;
    //                 string json = JsonConvert.SerializeObject(playerJson);
    //                 WebSocket.SendText(json);
    //             }
    //         }
    //     }
    // }

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

class PlayerJSON
{
    // public string Token { get; set; }
    // public bool IsServer { get; set; }
    // public string Username { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public float RotX { get; set; }
    public float RotY { get; set; }
    public float RotZ { get; set; }
}

public class PlayerPacket
{
    public string Token { get; set; } // Server-side
    public string Username { get; set; } // Server-side
    public bool IsServer { get; set; } // Server-side
    public string Dest { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public float RotX { get; set; }
    public float RotY { get; set; }
    public float RotZ { get; set; }
}

public class VoicePacket
{
    public string Token { get; set; } // Server-side
    public string Username { get; set; } // Server-side
    public bool IsServer { get; set; } // Server-side
    public string Dest { get; set; }
    public byte[] Data { get; set; }
    public bool IsP2P { get; set; }
}