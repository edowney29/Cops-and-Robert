using UnityEngine;

public class CrateController : MonoBehaviour
{
    public Crate crate;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Other Player"))
        {
            other.GetComponent<OtherController>().crateList.Add(crate.Id);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Other Player"))
        {
            other.GetComponent<OtherController>().crateList.Remove(crate.Id);
        }
    }
}
