using UnityEngine;

public class CrateSetter : MonoBehaviour
{
    public string Id { get; private set; }

    void Start()
    {
        Id = System.Guid.NewGuid().ToString();
    }

    public void UpdateId(string id)
    {
        Id = id;
    }
}
