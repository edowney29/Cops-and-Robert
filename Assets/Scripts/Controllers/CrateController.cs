using UnityEngine;

public class CrateController : MonoBehaviour
{
    public Crate Crate { get; private set; }
    public bool inTrigger = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("OtherPlayer"))
        {
            other.GetComponent<OtherController>().crateList.Add(Crate.Id);
        }

        if (other.gameObject.tag.Equals("Player"))
        {
            // other.GetComponent<ActionController>().crateList.Add(crate.Id);
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
            // other.GetComponent<ActionController>().crateList.Remove(crate.Id);
            inTrigger = false;
        }
    }

    public void SetCrate(Crate crate)
    {
        Crate = crate;
    }
}
