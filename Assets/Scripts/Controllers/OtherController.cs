using System.Collections.Generic;
using UnityEngine;

public class OtherController : MonoBehaviour
{
    string username;
    Vector3 realPosition, lastRealPosition;
    Quaternion realRotation, lastRealRotation;
    float destroyTimer = 0f, waitTime = 0.33333334f, spinTime = 0.22222223f, timeStartedLerping = 0f;
    bool isLerpingPosition = false, isLerpingRotation = false;
    public List<string> crateList = new List<string>();

    void Update()
    {
        destroyTimer += Time.deltaTime;
        if (destroyTimer > 5f) gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (isLerpingPosition)
        {
            float lerpPercentage = (Time.time - timeStartedLerping) / waitTime;
            transform.position = Vector3.Lerp(lastRealPosition, realPosition, lerpPercentage);
        }
        if (isLerpingRotation)
        {
            float lerpPercentage = (Time.time - timeStartedLerping) / spinTime;
            transform.rotation = Quaternion.Lerp(lastRealRotation, realRotation, lerpPercentage);
        }
    }

    public void UpdateTransform(PlayerPacket packet)
    {
        timeStartedLerping = Time.time;
        lastRealPosition = transform.position;
        lastRealRotation = transform.rotation;
        realPosition = new Vector3(packet.PosX, packet.PosY, packet.PosZ);
        realRotation = Quaternion.Euler(packet.RotX, packet.RotY, packet.RotZ);
        if (realPosition != transform.position)
        {
            isLerpingPosition = true;
        }
        if (realRotation.eulerAngles != transform.rotation.eulerAngles)
        {
            isLerpingRotation = true;
        }

        username = packet.Username;
        destroyTimer = 0f;
    }
}
