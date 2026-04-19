using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyItem : MonoBehaviour
{
    [Header("Références")]
    public OutlineController outlineController;

    private bool isPickedUp = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnPickUp);

        outlineController = GetComponent<OutlineController>();
    }

    void OnPickUp(SelectEnterEventArgs args)
    {
        if (isPickedUp) return;
        isPickedUp = true;

        // Désactiver l'outline une fois prise
        if (outlineController) outlineController.SetOutline(false);

        // Notifier le GameManager
        GameManager.Instance.SetState(GameState.HasKey);
    }

    public bool IsPickedUp() => isPickedUp;
}
