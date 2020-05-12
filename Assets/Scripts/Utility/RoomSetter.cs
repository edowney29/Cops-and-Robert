using UnityEngine;

public class RoomSetter : MonoBehaviour
{
    [SerializeField]
    string token, roomName;

    //When the Primitive collides with the walls, it will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            var comms = FindObjectOfType<Dissonance.DissonanceComms>();
            if (comms)
            {
                comms.RemoveToken("ayy");
                comms.AddToken(token);
            }

            var network = FindObjectOfType<NetworkController>();
            if (network)
            {
                network.SetLocation(roomName);
            }
        }
    }

    //When the Primitive exits the collision, it will change Color
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            var comms = FindObjectOfType<Dissonance.DissonanceComms>();
            if (comms)
            {
                comms.AddToken("ayy");
                comms.RemoveToken(token);
            }
        }

        var network = FindObjectOfType<NetworkController>();
        if (network)
        {
            network.SetLocation("Proximity");
        }
    }
}
