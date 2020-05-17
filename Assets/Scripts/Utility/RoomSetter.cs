using UnityEngine;

public class RoomSetter : MonoBehaviour
{
    [SerializeField]
    string token, roomName;
    NetworkManager networkManager;

    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
    }

    // When the Primitive collides with the walls, it will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            networkManager.SetLocation(roomName);
            // networkManager.comms.AddToken(token);
            networkManager.comms.RemoveToken("ayy");
        }
    }

    // When the Primitive exits the collision, it will change Color
    private void OnTriggerExit(Collider other)
    {
          if (other.gameObject.tag.Equals("Player"))
        {
            networkManager.SetLocation("Proximity");
            networkManager.comms.AddToken("ayy");
            // networkManager.comms.RemoveToken(token);
        }
    }
}
