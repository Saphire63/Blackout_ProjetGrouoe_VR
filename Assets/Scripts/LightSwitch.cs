using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LightSwitch : MonoBehaviour
{
    public PowerOutage powerOutage;
    private bool hasBeenUsed = false;

    void Awake()
    {
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable != null)
            interactable.selectEntered.AddListener(OnSwitchFlip);
    }

    void OnSwitchFlip(SelectEnterEventArgs args)
    {
        if (hasBeenUsed || powerOutage == null) return;
        hasBeenUsed = true;
        // Allume brièvement puis déclenche la coupure
        powerOutage.TurnOnThenOutage();
        GameManager.Instance.SetState(GameState.PowerOn);
    }
}