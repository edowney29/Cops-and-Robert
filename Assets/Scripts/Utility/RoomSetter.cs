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
            var vbt = other.gameObject.GetComponent<Dissonance.VoiceBroadcastTrigger>();
            if (vbt)
            {
                vbt.RoomName = roomName;
                vbt.enabled = true;
            }
        }
    }

    // When the Primitive exits the collision, it will change Color
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            networkController.SetLocation("Proximity");
            var vbt = other.gameObject.GetComponent<Dissonance.VoiceBroadcastTrigger>();
            if (vbt)
            {
                vbt.RoomName = "Global";
                vbt.enabled = false;
            }
        }
    }
}
