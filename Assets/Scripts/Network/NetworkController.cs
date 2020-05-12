using NativeWebSocket;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class NetworkController : MonoBehaviour
{
    [SerializeField]
    GameObject prefabPlayer, prefabOtherPlayer, overviewCamera, menuPanel, locationPanel, usernameInput, locationText;
    GameObject player = null;
    PlayerJSON playerJson = new PlayerJSON();
    Dictionary<string, OtherController> otherPlayers = new Dictionary<string, OtherController>();

    public string token = null;
    public string username = null;
    public bool isServer = false;
    public List<VoicePacket> voiceHolderClient = new List<VoicePacket>();
    public List<VoicePacket> voiceHolderServer = new List<VoicePacket>();

    public WebSocket WebSocket { get; private set; }

    void Start()
    {
        usernameInput.GetComponent<TMP_InputField>().onValueChanged.AddListener(SetUsername);

        WebSocket = new WebSocket("ws://cops-and-robert-server.herokuapp.com/ws");

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
            token = null;
            var comms = GetComponent<Dissonance.DissonanceComms>();
            if (comms)
            {
                comms.enabled = false;
            }
            locationPanel.SetActive(false);
            menuPanel.SetActive(true);
            Destroy(player);
        };

        WebSocket.OnMessage += (bytes) =>
        {
            string jsonHolder = Encoding.UTF8.GetString(bytes);
            // Debug.Log(jsonHolder);
            // foreach (string json in jsonHolder.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("}{", "}|{").Split('|'))
            foreach (string json in Regex.Replace(jsonHolder, "/(\r\n)|\n|\r/gm", "|").Split('|'))
            // foreach (string json in jsonHolder.Split('|'))
            {
#if UNITY_EDITOR
                // var length = json.Length;
                // if (length > 100) length = 100;
                // Debug.Log(json.Substring(0, length));
#endif
                if (json.Contains("Username"))
                {
                    PlayerPacket packet = JsonConvert.DeserializeObject<PlayerPacket>(json);
                    if (packet.Token != playerJson.Token)
                    {
                        if (otherPlayers.TryGetValue(packet.Token, out OtherController oc))
                        {
                            oc.UpdateTransform(packet);
                        }
                        else
                        {
                            GameObject obj = Instantiate(prefabOtherPlayer, new Vector3(packet.X, packet.Y, packet.Z), Quaternion.Euler(packet.RX, packet.RY, packet.RZ));
                            obj.name = packet.Token;
                            obj.GetComponent<VoiceController>().StartPlayer(packet.Token);
                            obj.GetComponent<Dissonance.VoiceBroadcastTrigger>().PlayerId = packet.Token;
                            otherPlayers.Add(packet.Token, obj.GetComponent<OtherController>());
                        }
                    }
                }
                else
                {
                    VoicePacket packet = JsonConvert.DeserializeObject<VoicePacket>(json);
                    if (isServer && !packet.IsServer && !packet.IsP2P)
                    {
                        voiceHolderServer.Add(packet);
                    }
                    else if (!isServer && packet.IsServer)
                    {
                        voiceHolderClient.Add(packet);
                    }
                    else if (isServer && packet.IsServer)
                    {
                        voiceHolderClient.Add(packet);
                    }
                    else
                    {
                        // voiceHolderServer.Add(packet);
                    }
                }
            }
        };


        //InvokeRepeating("SendPlayerJSON", 1f, 0.33333333f);
        StartCoroutine("SendPlayerJSON", 0.33333333f);

        WebSocket.Connect();
    }

    public void SpawnPlayer(bool isServer)
    {
        this.isServer = isServer;
        // overviewCamera.SetActive(false);
        token = Guid.NewGuid().ToString();
        player = Instantiate(prefabPlayer);
        // player.name = token;
        player.GetComponent<VoiceController>().StartPlayer(token);
        var comms = GetComponent<Dissonance.DissonanceComms>();
        if (comms)
        {
            comms.LocalPlayerName = token;
            comms.enabled = true;
        }
        locationPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    IEnumerator SendPlayerJSON(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            if (WebSocket.State == WebSocketState.Open && player != null && token != null)
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

    void OnApplicationQuit()
    {
        WebSocket.Close();
    }

    public void ToggleMic(bool toggle)
    {
        // enableMic = toggle;
    }

    public void SetUsername(string username)
    {
        this.username = username;
    }

    public void SetLocation(string location)
    {
        locationText.GetComponent<TMP_Text>().SetText(location);
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

// struct VoiceJSON
// {
//     public string Token { get; set; }
//     public float[] Data { get; set; }
//     public bool isServer { get; set; }
// }

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
    // public float[] Data { get; set; }
    public byte[] Data { get; set; }
    public bool IsServer { get; set; }
    public bool IsP2P { get; set; }
    public CustomConn Conn { get; set; }
}