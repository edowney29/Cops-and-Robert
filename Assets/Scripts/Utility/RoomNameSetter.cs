using UnityEngine;

public class RoomNameSetter : MonoBehaviour
{
    [SerializeField]
    string token, roomName;

    NetworkManager networkManager;
    InterfaceManager interfaceManager;

    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        interfaceManager = FindObjectOfType<InterfaceManager>();
    }

    // When the Primitive collides with the walls, it will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            interfaceManager.SetLocation(roomName);
            // networkManager.comms.AddToken(token);
            networkManager.comms.RemoveToken("ayy");
        }
    }

    // When the Primitive exits the collision, it will change Color
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            interfaceManager.SetLocation("Proximity");
            networkManager.comms.AddToken("ayy");
            // networkManager.comms.RemoveToken(token);
        }
    }
}
