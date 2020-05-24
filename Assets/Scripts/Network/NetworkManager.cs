using NativeWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : GameManager
{
    [SerializeField]
    GameObject prefabPlayer, prefabOtherPlayer, prefabCrate, prefabCopCrate, prefabRobCrate;
    GameObject player;

    PlayerJson playerJson = new PlayerJson();
    Dictionary<string, OtherController> otherPlayers = new Dictionary<string, OtherController>();
    Dictionary<string, CrateController> gameCrates = new Dictionary<string, CrateController>();
    public List<PlayerPacket> voiceHolderClient = new List<PlayerPacket>();
    public List<PlayerPacket> voiceHolderServer = new List<PlayerPacket>();

    InterfaceManager interfaceManager { get; set; }
    public Dissonance.DissonanceComms comms { get; private set; }
    public WebSocket WebSocket { get; private set; }
    public string Token { get; private set; }
    public string ServerToken { get; private set; }
    public bool IsServer { get; private set; }

    void Start()
    {
        interfaceManager = GetComponent<InterfaceManager>();
        comms = GetComponent<Dissonance.DissonanceComms>();

        InvokeRepeating("SendPlayerJSON", 0f, 0.33333334f);
        InvokeRepeating("SendGameJSON", 0f, 1f);
    }

    public async void StartWebSocket()
    {
        WebSocket = new WebSocket("ws://cops-and-robert-server.herokuapp.com/ws/" + interfaceManager.RoomId);

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
            // Destroy(player);
            // Destroy(Camera.main.gameObject);
            interfaceManager.ShowMenu();
        };

        WebSocket.OnMessage += (bytes) =>
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            // foreach (string json in jsonHolder.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("}{", "}|{").Split('|'))
            // foreach (string json in Regex.Replace(jsonHolder, "/(\r\n)|\n|\r/gm", "|").Split('|'))
            // foreach (string json in jsonHolder.Split('|'))
            // {          
            PlayerPacket packet = JsonConvert.DeserializeObject<PlayerPacket>(json);
            // Debug.Log("[PACKET]: TYPE_" + packet.Type + " --- " + packet.Token + " --- " + packet.IsServer);
            if (packet.IsServer) ServerToken = packet.Token;
            if (Token == null)
            {
                Token = packet.Token;
                // IsServer = packet.IsServer;
                // username = packet.Username;
                SpawnPlayer();
            }

            IsServer = Token.Equals(ServerToken);
            if (IsServer)
                if (packet.Type == PacketType.Player)
                {
                    if (IsServer) return;
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
                else if (packet.Type == PacketType.GameState)
                {
                    // Debug.Log(packet.Crates);
                    if (IsServer && !packet.IsServer)
                    {
                        if (otherPlayers.TryGetValue(packet.Token, out OtherController oc))
                        {
                            if (UpdateGameState(packet, oc))
                            {
                                SendGameJSON();
                            }
                        }
                    }
                    else if (packet.IsServer)
                    {
                        int exportScore = 0;
                        Crate[] crates = JsonConvert.DeserializeObject<Crate[]>(packet.Crates);
                        foreach (var crate in crates)
                        {
                            if (crate.Role == RoleCode.Null)
                            {
                                Debug.Log(crate.Id);
                                // if (gameCrates.TryGetValue(crate.Id, out CrateController cc))
                                // {
                                //     cc.crate = crate;
                                //     exportScore += crate.Score;
                                //     // interfaceManager
                                // }
                                // else
                                // {
                                //     var prefab = prefabCrate;
                                //     if (crate.Access == AccessCode.Cops) prefab = prefabCopCrate;
                                //     if (crate.Access == AccessCode.Robs) prefab = prefabRobCrate;
                                //     GameObject obj = Instantiate(prefab, new Vector3(packet.PosX, packet.PosY, packet.PosZ), Quaternion.Euler(packet.RotX, packet.RotY, packet.RotZ));
                                //     // obj.name = packet.Token;
                                //     gameCrates.Add(packet.Token, obj.GetComponent<CrateController>());
                                // }
                            }
                        }
                    }
                    else
                    {

                    }
                }
                else
                {

                }
        };

        await WebSocket.Connect();
    }

    public void StartGame()
    {
        if (isRunning)
            ResetGameState();
        else
            SetupGameState(otherPlayers, Token);
    }

    void SpawnPlayer()
    {
        comms.LocalPlayerName = Token;
        comms.enabled = true;
        player = Instantiate(prefabPlayer);
        // player.name = Token;
        player.GetComponent<VoiceController>().StartVoice(Token);
        interfaceManager.ShowGame();
    }

    async void SendPlayerJSON()
    {
        if (WebSocket != null && player != null && Token != null)
        {
            playerJson.UpdateTransform(player.transform);
            if (WebSocket.State == WebSocketState.Open)
            {
                string json = JsonConvert.SerializeObject(playerJson);
                await WebSocket.SendText(json);
            }
        }
    }

    async void SendGameJSON()
    {
        if (IsServer)
        {
            interfaceManager.SetupIsServerView();
            if (WebSocket != null && Token != null && isRunning)
            {
                if (WebSocket.State == WebSocketState.Open)
                {
                    Crate[] crates = new Crate[cratesHolder.Count];
                    cratesHolder.Values.CopyTo(crates, 0);
                    var packet = new GameStateJson(JsonConvert.SerializeObject(crates));
                    string json = JsonConvert.SerializeObject(packet);
                    await WebSocket.SendText(json);
                }
            }
        }
    }

    public void ToggleMic(bool toggle)
    {
        // enableMic = toggle;
    }
}

public enum PacketType
{
    Player,
    Voice,
    GameState,
}

public class PlayerJson
{
    public PacketType Type { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public float RotX { get; set; }
    public float RotY { get; set; }
    public float RotZ { get; set; }

    public PlayerJson()
    {
        Type = PacketType.Player;
    }

    public void UpdateTransform(Transform transform)
    {
        PosX = transform.position.x;
        PosY = transform.position.y;
        PosZ = transform.position.z;
        RotX = transform.eulerAngles.x;
        RotY = transform.eulerAngles.y;
        RotZ = transform.rotation.eulerAngles.z;
    }
}

public class VoiceJson
{
    public PacketType Type { get; set; }
    public string Dest { get; set; }
    public byte[] Data { get; set; }
    public bool IsP2P { get; set; }

    public VoiceJson(string dest, byte[] data, bool isP2P)
    {
        Type = PacketType.Voice;
        Dest = dest;
        Data = data;
        IsP2P = isP2P;
    }
}

public class GameStateJson
{
    public PacketType Type { get; set; }
    public string Crates { get; set; }
    public GameStateJson(string crates)
    {
        Type = PacketType.GameState;
        Crates = crates;
    }
}

public class PlayerPacket
{
    public PacketType Type { get; set; }
    public string Token { get; set; }
    public string Username { get; set; }
    public string Dest { get; set; }
    public bool IsServer { get; set; }
    public bool IsP2P { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public float RotX { get; set; }
    public float RotY { get; set; }
    public float RotZ { get; set; }
    public byte[] Data { get; set; }
    public ActionType Action { get; set; }
    public string Crates { get; set; }
}
