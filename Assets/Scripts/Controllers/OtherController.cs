using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OtherController : MonoBehaviour
{
    string username;
    Vector3 position, rotation;
    float destroyTimer = 0f, waitTime = 0.33333334f, spinTime = 0.22222223f;

    public List<string> crateList = new List<string>();

    void Start()
    {
        position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        rotation = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        InvokeRepeating("AsyncUpdate", 0f, waitTime);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Crate")
        {
            crateList.Add(other.gameObject.GetComponent<CrateController>().Id);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Crate")
        {
            crateList.Remove(other.gameObject.GetComponent<CrateController>().Id);
        }
    }

    void AsyncUpdate()
    {
        transform.DOMove(position, waitTime, false);
        transform.DORotate(rotation, spinTime, RotateMode.Fast);
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
