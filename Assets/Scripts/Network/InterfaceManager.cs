using TMPro;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    [SerializeField]
    GameObject menuPanel, locationPanel;
    [SerializeField]
    TMP_InputField usernameInput, passwordInput;
    [SerializeField]
    TMP_Text locationText;

    public string RoomId { get; private set; }
    public string Username { get; private set; }

    void Start()
    {
        usernameInput.onValueChanged.AddListener(SetUsername);
        passwordInput.onValueChanged.AddListener(SetRoomId);

        locationPanel.SetActive(false);
    }

    void SetUsername(string username)
    {
        Username = username;
    }

    void SetRoomId(string roomId)
    {
        RoomId = roomId;
    }

    public void SetLocation(string location)
    {
        locationText.SetText(location);
    }

    public void ShowMenu()
    {
        locationPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void ShowGame()
    {
        locationPanel.SetActive(true);
        menuPanel.SetActive(false);
    }
}
