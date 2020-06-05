using UnityEngine;

public class RoomSetter : MonoBehaviour
{
    [SerializeField]
    string token, roomName;

    // NetworkManager networkManager;
    Dissonance.DissonanceComms comms;
    InterfaceController gui;

    void Start()
    {
        // networkManager = FindObjectOfType<NetworkManager>();
        comms = FindObjectOfType<Dissonance.DissonanceComms>();
        gui = FindObjectOfType<InterfaceController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            gui.SetLocation(roomName);
            comms.AddToken(token);
            comms.RemoveToken("ayy");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            gui.SetLocation("Proximity");
            comms.AddToken("ayy");
            comms.RemoveToken(token);
        }
    }
}
