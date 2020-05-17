using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherController : MonoBehaviour
{
    string username;
    Vector3 position, rotation;
    float destroyTimer = 0f, waitTime = 0.33f, spinTime = 0.22f;

    public List<string> crateList = new List<string>();

    void Start()
    {
        // GameObject sign = new GameObject("player_label");
        // sign.transform.rotation = Camera.main.transform.rotation; // Causes the text faces camera.
        // TextMesh tm = sign.AddComponent<TextMesh>();
        // tm.text = username;
        // tm.color = new Color(0.8f, 0.8f, 0.8f);
        // tm.fontStyle = FontStyle.Bold;
        // tm.alignment = TextAlignment.Center;
        // tm.anchor = TextAnchor.MiddleCenter;
        // tm.characterSize = 0.065f;
        // tm.fontSize = 60;

        position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        rotation = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        InvokeRepeating("AsyncUpdate", 0f, 0.33333333f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Crate")
        {
            crateList.Add(other.gameObject.GetComponent<CrateSetter>().Id);
        }
    }

    // When the Primitive exits the collision, it will change Color
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Crate")
        {
            crateList.Remove(other.gameObject.GetComponent<CrateSetter>().Id);
        }
    }

    void Update()
    {
        destroyTimer += Time.deltaTime;

        if (destroyTimer > 5f)
        {
            Debug.LogWarning(gameObject.name);
            Destroy(gameObject);
        }
    }

    void AsyncUpdate()
    {
        LeanTween.move(gameObject, position, waitTime);
        LeanTween.rotate(gameObject, rotation, spinTime);
    }

    public void UpdateTransform(PlayerPacket packet)
    {
        username = packet.Username;
        position.x = packet.PosX;
        position.y = packet.PosY;
        position.z = packet.PosZ;
        rotation.x = packet.RotX;
        rotation.y = packet.RotY;
        rotation.z = packet.RotZ;
        destroyTimer = 0f;
    }
}
