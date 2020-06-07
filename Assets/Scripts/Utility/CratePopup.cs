using UnityEngine;

public class CratePopup : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text display, drugs, evidence, warrants;
    [SerializeField]
    UnityEngine.UI.Image image;
    [SerializeField]
    GameObject buttonWarrant;

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
        warrants.enabled = shouldShow;
        image.enabled = shouldShow;

        if (shouldShow)
        {
            if (Input.GetKeyDown(KeyCode.F) && !buttonPressed)
            {
                buttonPressed = true;
                networkManager.ValidateAction(crateController.Crate, InputType.F);
            }
            else if (Input.GetKeyDown(KeyCode.R) && !buttonPressed)
            {
                buttonPressed = true;
                networkManager.ValidateAction(crateController.Crate, InputType.R);
            }
            else if (Input.GetKeyDown(KeyCode.C) && !buttonPressed)
            {
                buttonPressed = true;
                networkManager.ValidateAction(crateController.Crate, InputType.C);
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
            warrants.SetText("Warrants: " + crateController.Crate.Warrants);

            if (networkManager.MyCrate.Access == AccessCode.Cops && crateController.Crate.IsExport)
            {
                display.SetText("Crate");
            }
            if (networkManager.MyCrate.Access == AccessCode.Robs)
            {
                warrants.enabled = false;
                buttonWarrant.SetActive(false);
            }
            if (networkManager.MyCrate.Access == AccessCode.Cops && (networkManager.MyCrate.Role == RoleCode._1 || networkManager.MyCrate.Role == RoleCode._2))
            {
                drugs.enabled = false;
            }
            if (networkManager.MyCrate.Access == AccessCode.Robs && (networkManager.MyCrate.Role == RoleCode._1 || networkManager.MyCrate.Role == RoleCode._2))
            {
                evidence.enabled = false;
            }
            if (networkManager.MyCrate.Access == AccessCode.Robs && networkManager.MyCrate.Role == RoleCode._1 && crateController.Crate.IsExport)
            {
                display.enabled = true;
            }
            // if (networkManager.MyCrate.Access == AccessCode.Cops && networkManager.MyCrate.Role == RoleCode._1 &&)
            // {
            //     display.enabled = true;
            // }
        }
    }

    public void CreateWarrant()
    {
        networkManager.ValidateAction(crateController.Crate, InputType.CreateWarrant);
    }

    public void UseWarrant()
    {
        networkManager.ValidateAction(crateController.Crate, InputType.UseWarrant);
    }
}
