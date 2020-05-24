using NativeWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : GameManager
{
    [SerializeField]
    GameObject prefabPlayer, prefabOtherPlayer, prefabCrate, prefabCopCrate, prefabRobCrate, prefabCratePopup;
    [SerializeField]
    Material blueVisor, redVisor;

    GameObject player;
    Crate playerCrate;
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

        InvokeRepeating("SendPlayerJson", 0f, 0.33333334f);
        InvokeRepeating("SendGameJson", 0f, 1f);
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
            Debug.Log("[PACKET]: TYPE_" + packet.Type + " --- " + packet.Token + " --- " + packet.IsServer);
            if (packet.IsServer) ServerToken = packet.Token;
            if (Token == null)
            {
                Token = packet.Token;
                if (packet.IsServer) interfaceManager.SetupIsServerView();
                // username = packet.Username;
                SpawnPlayer();
            }

            IsServer = Token.Equals(ServerToken);
            if (packet.Type == PacketType.Player)
            {
                if (Token.Equals(packet.Token)) return;
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
                int exportScore = 0;
                Crate[] crates = JsonConvert.DeserializeObject<Crate[]>(packet.Crates);
                foreach (var crate in crates)
                {
                    if (Token.Equals(crate.Id))
                    {
                        playerCrate = crate;
                        interfaceManager.DrugsText(crate.Drugs.ToString());
                        interfaceManager.EvidenceText(crate.Evidence.ToString());
                        if (crate.Access == AccessCode.Robs && crate.Role == RoleCode._1)
                        {

                        }
                    }

                    if (crate.Role == RoleCode.Null)
                    {
                        exportScore += crate.Score;
                        if (gameCrates.TryGetValue(crate.Id, out CrateController cc))
                        {
                            cc.SetCrate(crate);
                        }
                        else
                        {
                            var prefab = prefabCrate;
                            if (crate.Access == AccessCode.Cops) prefab = prefabCopCrate;
                            if (crate.Access == AccessCode.Robs) prefab = prefabRobCrate;
                            GameObject obj = Instantiate(prefab, new Vector3(crate.PosX, crate.PosY, crate.PosZ), Quaternion.Euler(crate.RotX, crate.RotY, crate.RotZ));
                            var crateController = obj.GetComponent<CrateController>();
                            crateController.SetCrate(crate);

                            GameObject popup = Instantiate(prefabCratePopup, interfaceManager.locationPanel.transform);
                            var cratePopup = popup.GetComponent<CratePopup>();
                            cratePopup.crateController = crateController;
                            cratePopup.networkManager = this;
                            // obj.name = crate.Id;
                            gameCrates.Add(crate.Id, crateController);
                        }
                    }
                }
                interfaceManager.ExportsText(exportScore.ToString());
            }
            else if (packet.Type == PacketType.Action)
            {
                if (otherPlayers.TryGetValue(packet.Token, out OtherController oc))
                {
                    UpdateGameState(packet, oc);
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
        {
            ResetGameState();
            interfaceManager.StartButtonText("Start Game");
        }
        else
        {
            SetupGameState(otherPlayers, Token);
            interfaceManager.StartButtonText("Reset Game");
        }
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

    async void SendPlayerJson()
    {
        if (WebSocket != null && Token != null && player != null)
        {
            playerJson.UpdateTransform(player.transform);
            if (WebSocket.State == WebSocketState.Open)
            {
                string json = JsonConvert.SerializeObject(playerJson);
                await WebSocket.SendText(json);
            }
        }
    }

    async void SendGameJson()
    {
        if (WebSocket != null && Token != null && isRunning)
        {
            if (WebSocket.State == WebSocketState.Open)
            {
                var cratesHolder = GetCratesHolder();
                Crate[] crates = new Crate[cratesHolder.Count];
                cratesHolder.Values.CopyTo(crates, 0);
                var packet = new GameStateJson(JsonConvert.SerializeObject(crates));
                string json = JsonConvert.SerializeObject(packet);
                await WebSocket.SendText(json);
            }
        }
    }

    async void SendActionJson(ActionType action)
    {
        var packet = new ActionJson(action, ServerToken);
        string json = JsonConvert.SerializeObject(packet);
        await WebSocket.SendText(json);
    }

    public void ValidateAction(CrateController crate, AccessCode access)
    {
        if (playerCrate != null)
        {
            var action = DetermineAction(playerCrate, crate.Crate, access);
            if (IsServer)
            {
                UpdateGameStateServer(playerCrate.Id, crate.Crate.Id, action);
            }
            else
            {
                SendActionJson(action);
            }
        }
    }

    ActionType DetermineAction(Crate player, Crate crate, AccessCode access)
    {
        // TODO: Use crates for skill checking?
        if (player.Drugs > 0 && access == AccessCode.Robs) return ActionType.StoreDrugs;
        if (player.Drugs == 0 && access == AccessCode.Robs) return ActionType.GetDrugs;
        if (player.Evidence > 0 && access == AccessCode.Cops) return ActionType.StoreEvidence;
        if (player.Evidence == 0 && access == AccessCode.Cops) return ActionType.GetEvidence;
        return ActionType.Null;
    }
}

public enum PacketType
{
    Player,
    Voice,
    GameState,
    Action
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

public class ActionJson
{
    public PacketType Type { get; set; }
    public ActionType Action { get; set; }
    public string Dest { get; set; }

    public ActionJson(ActionType action, string dest)
    {
        Type = PacketType.Action;
        Action = action;
        Dest = dest;
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
