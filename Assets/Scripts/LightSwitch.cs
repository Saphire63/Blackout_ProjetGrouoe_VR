using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LightSwitch : MonoBehaviour
{
    public PowerOutage powerOutage;
    private bool hasBeenUsed = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnSwitchFlip);
    }

    void OnSwitchFlip(SelectEnterEventArgs args)
    {
        if (hasBeenUsed) return;
        hasBeenUsed = true;

        GameManager.Instance.SetState(GameState.PowerOn);
        powerOutage.TriggerOutage();
    }
}
