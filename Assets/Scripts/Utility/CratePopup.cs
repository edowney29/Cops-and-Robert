using UnityEngine;

public class CratePopup : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text display, drugs, evidence;
    [SerializeField]
    UnityEngine.UI.Image image;

    bool buttonPressed = false;

    public CrateController crateController;
    public NetworkManager networkManager;

    void Update()
    {
        if (!crateController.isActiveAndEnabled) gameObject.SetActive(false);

        var screenPoint = Camera.main.WorldToScreenPoint(crateController.transform.position);
        GetComponent<RectTransform>().position = screenPoint;

        var veiwportPoint = Camera.main.WorldToViewportPoint(crateController.transform.position);
        var distanceFromCenter = Vector2.Distance(veiwportPoint, Vector2.one * 0.5f);

        var show = distanceFromCenter < 0.4f;
        var shouldShow = show && crateController.inTrigger;
        display.enabled = shouldShow;
        drugs.enabled = shouldShow;
        evidence.enabled = shouldShow;
        image.enabled = shouldShow;

        if (shouldShow)
        {
            if (Input.GetKeyDown(KeyCode.F) && !buttonPressed)
            {
                buttonPressed = true;
                networkManager.ValidateAction(crateController, AccessCode.Robs);
            }
            else if (Input.GetKeyDown(KeyCode.R) && !buttonPressed)
            {
                buttonPressed = true;
                networkManager.ValidateAction(crateController, AccessCode.Cops);
            }
            else
            {
                buttonPressed = false;
            }
        }
        else
        {
            buttonPressed = false;
        }

        if (crateController.Crate != null && networkManager.MyCrate != null)
        {
            display.SetText(crateController.Crate.Display);
            drugs.SetText("Drugs: " + crateController.Crate.Drugs);
            evidence.SetText("Evidence: " + crateController.Crate.Evidence);

            if (networkManager.MyCrate.Access == AccessCode.Robs && networkManager.MyCrate.Role == RoleCode._1 && crateController.Crate.IsExport)
            {
                display.enabled = true;
            }
            if (networkManager.MyCrate.Access == AccessCode.Cops && crateController.Crate.IsExport)
            {
                display.SetText("Crate");
            }
            if (networkManager.MyCrate.Access == AccessCode.Cops)
            {
                drugs.enabled = false;
            }
            if (networkManager.MyCrate.Access == AccessCode.Robs)
            {
                evidence.enabled = false;
            }
        }
    }
}
