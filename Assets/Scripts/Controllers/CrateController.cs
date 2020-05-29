using UnityEngine;

public class CrateController : MonoBehaviour
{
    public Crate Crate { get; private set; }
    public bool inTrigger = false;
    float destroyTimer = float.MinValue;

    void Update()
    {
        destroyTimer += Time.deltaTime;
        if (destroyTimer > 5f) gameObject.SetActive(false);
        // if (destroyTimer > 5f) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("OtherPlayer"))
        {
            other.GetComponent<OtherController>().crateList.Add(Crate.Id);
        }

        if (other.gameObject.tag.Equals("Player"))
        {
            inTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("OtherPlayer"))
        {
            other.GetComponent<OtherController>().crateList.Remove(Crate.Id);
        }

        if (other.gameObject.tag.Equals("Player"))
        {
            inTrigger = false;
        }
    }

    public void SetCrate(Crate crate)
    {
        Crate = crate;
        destroyTimer = 0f;
    }
}
