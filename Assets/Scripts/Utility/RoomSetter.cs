using UnityEngine;

public class RoomSetter : MonoBehaviour
{
    [SerializeField]
    string token, roomName;
    NetworkController networkController;

    void Start()
    {
        networkController = FindObjectOfType<NetworkController>();
    }

    // When the Primitive collides with the walls, it will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            networkController.SetLocation(roomName);
            // networkController.comms.AddToken(token);
            networkController.comms.RemoveToken("ayy");
        }
    }

    // When the Primitive exits the collision, it will change Color
    private void OnTriggerExit(Collider other)
    {
          if (other.gameObject.tag.Equals("Player"))
        {
            networkController.SetLocation("Proximity");
            networkController.comms.AddToken("ayy");
            // networkController.comms.RemoveToken(token);
        }
    }
}
