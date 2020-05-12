using Dissonance;
using UnityEngine;

public class VoiceController : MonoBehaviour, IDissonancePlayer
{
    [SerializeField]
    string playerId;
    DissonanceComms comms;

    public string PlayerId { get { return playerId; } }
    public Vector3 Position { get { return transform.position; } }
    public Quaternion Rotation { get { return transform.rotation; } }
    public bool IsTracking { get; private set; }
    public NetworkPlayerType Type
    {
        get
        {
            if (comms == null || playerId == null)
                return NetworkPlayerType.Unknown;
            return comms.LocalPlayerName.Equals(playerId) ? NetworkPlayerType.Local : NetworkPlayerType.Remote;
        }
    }

    void OnEnable()
    {
        comms = FindObjectOfType<DissonanceComms>();
    }

    void OnDisable()
    {
        if (IsTracking)
            StopTracking();
    }

    public void StartPlayer(string playerName)
    {
        SetPlayerName(playerName);
    }

    void SetPlayerName(string playerName)
    {
        //We need the player name to be set on all the clients and then tracking to be started (on each client).
        //To do this we send a command from this client, informing the server of our name. The server will pass this on to all the clients (with an RPC)
        // Client -> Server -> Client

        //We need to stop and restart tracking to handle the name change
        if (IsTracking)
            StopTracking();

        //Perform the actual work
        playerId = playerName;
        StartTracking();

        // //Inform the server the name has changed
        // if (isLocalPlayer)
        //     CmdSetPlayerName(playerName);
    }

    void StartTracking()
    {
        // if (IsTracking)
        //     throw Log.CreatePossibleBugException("Attempting to start player tracking, but tracking is already started", "B7D1F25E-72AF-4E93-8CFF-90CEBEAC68CF");

        if (comms != null)
        {
            comms.TrackPlayerPosition(this);
            IsTracking = true;
        }
    }

    void StopTracking()
    {
        // if (!IsTracking)
        //     throw Log.CreatePossibleBugException("Attempting to stop player tracking, but tracking is not started", "EC5C395D-B544-49DC-B33C-7D7533349134");

        if (comms != null)
        {
            comms.StopTracking(this);
            IsTracking = false;
        }
    }
}
