using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LightSwitch : MonoBehaviour
{
    public PowerOutage powerOutage;
    private bool hasBeenUsed = false;

    void Awake()
    {
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnSwitchFlip);
    }

    void OnSwitchFlip(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if (hasBeenUsed || powerOutage == null) return;
        hasBeenUsed = true;
        powerOutage.TriggerOutage();
    }
}