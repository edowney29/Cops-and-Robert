using NativeWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : GameManager
{
    [SerializeField]
    GameObject player, prefabOtherPlayer, prefabCrate, prefabCopCrate, prefabRobCrate, prefabCratePopup, prefabCopCratePopup, modelCop1, modelCop2, modelRob1, modelRob2; // prefabPlayer
    [SerializeField]
    Material blueVisor, redVisor;

    PlayerJson playerJson = new PlayerJson();
    Dictionary<string, OtherController> otherPlayers = new Dictionary<string, OtherController>();
    Dictionary<string, CrateController> gameCrates = new Dictionary<string, CrateController>();
    public List<PlayerPacket> voiceHolderClient = new List<PlayerPacket>();
    public List<PlayerPacket> voiceHolderServer = new List<PlayerPacket>();

    public string Token { get; private set; }
    public Dissonance.DissonanceComms Comms { get; private set; }
    public InterfaceController GUI { get; private set; }
    public Crate MyCrate { get; private set; }
    public WebSocket WebSocket { get; private set; }
    public string ServerToken { get; private set; }
    public bool IsServer { get; private set; }

    void Start()
    {
        if (SteamManager.Initialized)
        {
            playerJson.Username = Steamworks.SteamFriends.GetPersonaName();
        }

        GUI = GetComponent<InterfaceController>();
        Comms = GetComponent<Dissonance.DissonanceComms>();

        InvokeRepeating("SendPlayerJson", 0f, 0.33333334f);
        InvokeRepeating("SendGameJson", 0f, 1f);
    }

    public async void JoinGame()
    {
        WebSocket = new WebSocket("ws://cops-and-robert.herokuapp.com/ws/" + GUI.RoomId);

        WebSocket.OnOpen += () =>
        {
            ResetGameState();
            GUI.ResetAll();
            Debug.Log("Connection open!");
        };

        WebSocket.OnError += (e) =>
        {
            Debug.LogError("Error! " + e);
            WebSocket.Close();
        };

        WebSocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            Token = null;
            serverRunning = false;
            Comms.enabled = false;
            // Destroy(player);
            // Destroy(Camera.main.gameObject);
            player.SetActive(false);
            // Camera.main.gameObject.SetActive(false);
            GUI.SetIsRunning(false);
            GUI.ShowMenu();
        };

        WebSocket.OnMessage += (bytes) =>
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            // foreach (string json in jsonHolder.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("}{", "}|{").Split('|'))
            // foreach (string json in Regex.Replace(jsonHolder, "/(\r\n)|\n|\r/gm", "|").Split('|'))
            // foreach (string json in jsonHolder.Split('|'))
            // {                          
            PlayerPacket packet = JsonConvert.DeserializeObject<PlayerPacket>(json);
            Debug.Log("[PACKET]: " + packet.Type + " --- " + packet.Token + " --- " + packet.IsServer);
            if (packet.IsServer) ServerToken = packet.Token;

            if (packet.Type == PacketType.Player)
            {
                if (Token.Equals(packet.Token)) return;
                if (otherPlayers.TryGetValue(packet.Token, out OtherController oc))
                {
                    if (oc.isActiveAndEnabled) oc.UpdateTransform(packet);
                    else oc.gameObject.SetActive(true);
                    // TODO: Validate position based on gamestate
                }
                else
                {
                    GameObject obj = Instantiate(prefabOtherPlayer, new Vector3(packet.PosX, packet.PosY, packet.PosZ), Quaternion.Euler(packet.RotX, packet.RotY, packet.RotZ));
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
                else //if (packet.IsServer || packet.IsP2P)
                {
                    // Debug.Log("CLIENT: " + packet.Token + " - " + packet.IsServer + " - " + packet.IsP2P + " - " + comms.IsNetworkInitialized);
                    voiceHolderClient.Add(packet);
                }

            }
            else if (packet.Type == PacketType.GameState)
            {
                int exportScore = 0;
                Crate[] crates = JsonConvert.DeserializeObject<Crate[]>(packet.Crates);
                foreach (var crate in crates)
                {
                    // Is my Player
                    if (Token.Equals(crate.Id) && player.activeSelf)
                    {
                        MyCrate = crate;
                        GUI.DrugsText(crate.Drugs);
                        GUI.EvidenceText(crate.Evidence);
                        GUI.WarrantsText(crate.Warrants);
                        GUI.RoleNameText(crate.Access, crate.Role);
                        var meshes = player.GetComponentsInChildren<MeshRenderer>();
                        meshes[meshes.Length - 1].material = crate.Access == AccessCode.Cops ? blueVisor : redVisor;
                    }
                    // Is a Player
                    else if (crate.Role != RoleCode.Null)
                    {
                        if (otherPlayers.TryGetValue(crate.Id, out OtherController oc))
                        {
                            if (oc.isActiveAndEnabled)
                            {
                                var meshes = oc.GetComponentsInChildren<MeshRenderer>();
                                meshes[meshes.Length - 1].material = crate.Access == AccessCode.Cops ? blueVisor : redVisor;
                            }
                        }
                    }
                    // Is a Crate
                    else
                    {
                        exportScore += crate.Score;
                        if (gameCrates.TryGetValue(crate.Id, out CrateController cc))
                        {
                            if (cc.isActiveAndEnabled) cc.SetCrate(crate);
                        }
                        else
                        {
                            GameObject prefab, prefabPopup;
                            if (crate.Access == AccessCode.Cops)
                            {
                                prefab = prefabCopCrate;
                                prefabPopup = prefabCopCratePopup;
                            }
                            else if (crate.Access == AccessCode.Robs)
                            {
                                prefab = prefabRobCrate;
                                prefabPopup = prefabCratePopup;
                            }
                            else
                            {
                                prefab = prefabCrate;
                                prefabPopup = prefabCratePopup;
                            }

                            GameObject obj = Instantiate(prefab, new Vector3(crate.PosX, crate.PosY, crate.PosZ), Quaternion.Euler(crate.RotX, crate.RotY, crate.RotZ));
                            var crateController = obj.GetComponent<CrateController>();
                            crateController.SetCrate(crate);

                            GameObject popup = Instantiate(prefabPopup, GUI.locationPanel.transform);
                            var cratePopup = popup.GetComponent<CratePopup>();
                            cratePopup.crateController = crateController;
                            cratePopup.networkManager = this;
                            gameCrates.Add(crate.Id, crateController);
                        }
                    }
                }
                GUI.ExportsText(exportScore);
            }
            else if (packet.Type == PacketType.Action)
            {
                if (IsServer && otherPlayers.TryGetValue(packet.Token, out OtherController oc))
                {
                    if (oc.isActiveAndEnabled) UpdateGameState(packet, oc);
                }
            }
            else
            {
                if (Token == null)
                {
                    Token = packet.Token;
                    if (packet.IsServer)
                    {
                        IsServer = true;
                        GUI.SetupIsServerView();
                    }
                    GUI.SetIsRunning(true);
                    SpawnPlayer();
                }
            }
        };

        await WebSocket.Connect();
    }

    public void StartGame()
    {
        if (serverRunning) ResetGameState();
        else SetupGameState(otherPlayers, Token);
        GUI.StartButtonText(serverRunning);
    }

    public async void LeaveGame()
    {
        GUI.ToggleSettings();
        await WebSocket.Close();
    }

    void SpawnPlayer()
    {
        if (Comms.LocalPlayerName != Token) Comms.LocalPlayerName = Token;
        Comms.enabled = true;
        // player = Instantiate(prefabPlayer);
        player.SetActive(true);
        // Camera.main.gameObject.SetActive(true);
        player.GetComponent<VoiceController>().StartVoice(Token);
        GUI.ShowGame();
    }

    async void SendPlayerJson()
    {
        if (WebSocket != null && Token != null && player.activeSelf)
        {
            if (WebSocket.State == WebSocketState.Open)
            {
                playerJson.UpdateTransform(player.transform);
                string json = JsonConvert.SerializeObject(playerJson);
                await WebSocket.SendText(json);
            }
        }
    }

    async void SendGameJson()
    {
        if (WebSocket != null && Token != null && serverRunning)
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

    async void SendActionJson(ActionType action, string actionCrate)
    {
        var packet = new ActionJson(action, actionCrate, ServerToken);
        string json = JsonConvert.SerializeObject(packet);
        await WebSocket.SendText(json);
    }

    public void ValidateAction(Crate crate, InputType input)
    {
        if (MyCrate != null)
        {
            var action = DetermineAction(MyCrate, input);
            if (action != ActionType.Null)
            {
                if (IsServer) UpdateGameStateServer(MyCrate.Id, crate.Id, action);
                else SendActionJson(action, crate.Id);
            }
        }
    }
}

public enum PacketType
{
    Null,
    Player,
    Voice,
    GameState,
    Action
}

public class PlayerJson
{
    public PacketType Type { get; set; }
    public string Username { get; set; }
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
    public string ActionCrate { get; set; }
    public string Dest { get; set; }

    public ActionJson(ActionType action, string actionCrate, string dest)
    {
        Type = PacketType.Action;
        Action = action;
        ActionCrate = actionCrate;
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
    public string Crates { get; set; }
    public ActionType Action { get; set; }
    public string ActionCrate { get; set; }
}
