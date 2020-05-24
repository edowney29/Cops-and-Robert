using TMPro;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    public GameObject menuPanel, locationPanel, startGameButton, cratesObject;
    public TMP_InputField usernameInput, passwordInput;
    public TMP_Text locationText, drugsCountText, evidenceCountText, exportsCountText;

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

    public void SetupIsServerView()
    {
        startGameButton.SetActive(true);
        cratesObject.SetActive(true);
    }

    public void StartButtonText(string text)
    {
        startGameButton.GetComponentInChildren<TMP_Text>().SetText(text);
    }

    public void ExportsText(string text)
    {
        exportsCountText.SetText("Exports: " + text);
    }

    public void DrugsText(string text)
    {
        drugsCountText.SetText("Drugs: " + text);
    }

    public void EvidenceText(string text)
    {
        evidenceCountText.SetText("Evidence: " + text);
    }
}
