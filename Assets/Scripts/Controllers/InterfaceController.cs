using TMPro;
using UnityEngine;

public class InterfaceController : MonoBehaviour
{
    public GameObject menuPanel, locationPanel, settingsPanel, startGameButton;
    public TMP_InputField usernameInput, passwordInput;
    public TMP_Text locationText, drugsCountText, evidenceCountText, warrantsCountText, exportsCountText, roleNameText, timerText;

    bool buttonPressed = false, isRunning = false;

    public string RoomId { get; private set; }
    public string Username { get; private set; }

    void Start()
    {
        usernameInput.onValueChanged.AddListener(SetUsername);
        passwordInput.onValueChanged.AddListener(SetRoomId);

        settingsPanel.SetActive(false);
        locationPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    void Update()
    {
        if (!isRunning)
        {
            buttonPressed = false;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && !buttonPressed)
        {
            buttonPressed = true;
            ToggleSettings();
        }
        else
        {
            buttonPressed = false;
        }
    }

    public void SetIsRunning(bool value)
    {
        isRunning = value;
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

    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void SetupIsServerView()
    {
        startGameButton.SetActive(true);
    }

    public void StartButtonText(bool serverRunning)
    {
        if (serverRunning)
        {
            startGameButton.GetComponentInChildren<TMP_Text>().SetText("Reset Game");
        }
        else
        {
            startGameButton.GetComponentInChildren<TMP_Text>().SetText("Start Game");
        }
    }

    public void ExportsText(int count)
    {
        exportsCountText.SetText("Exports: " + count);
    }

    public void DrugsText(int count)
    {
        drugsCountText.SetText("Drugs: " + count);
    }

    public void WarrantsText(int count)
    {
        warrantsCountText.SetText("Warrants: " + count);
    }

    public void EvidenceText(int count)
    {
        evidenceCountText.SetText("Evidence: " + count);
    }

    public void RoleNameText(AccessCode access, RoleCode role)
    {
        string text = "Role Name";
        if (access == AccessCode.Cops)
        {
            if (role == RoleCode._1) text = "Sheriff";
            else if (role == RoleCode._2) text = "Police Office";
            else if (role == RoleCode._3) text = "Under Cover Cop";
            else if (role == RoleCode._4) text = "Informant";
            else if (role == RoleCode._5) text = "Rookie";
            else text = "Rookie";
        }
        if (access == AccessCode.Robs)
        {
            if (role == RoleCode._1) text = "Robert";
            else if (role == RoleCode._2) text = "Criminal";
            else if (role == RoleCode._3) text = "Mob Cop";
            else if (role == RoleCode._4) text = "Crooked Cop";
            else if (role == RoleCode._5) text = "Street Thug";
            else text = "Street Thug";
        }
        roleNameText.SetText(text);
    }

    public void ResetAll()
    {
        SetLocation("Proximity");
        EvidenceText(0);
        DrugsText(0);
        WarrantsText(0);
        ExportsText(0);
        roleNameText.SetText("Role Name");
    }
}
