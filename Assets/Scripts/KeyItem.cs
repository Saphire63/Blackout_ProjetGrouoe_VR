using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class KeyItem : MonoBehaviour
{
    [Header("Effets")]
    public AudioSource audioSource;
    public AudioClip pickupSound;

    private bool isPickedUp = false;
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        // On utilise XRGrabInteractable au lieu de XRSimpleInteractable
        // pour que la clé soit vraiment tenue en main
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (isPickedUp) return;

        isPickedUp = true;

        if (audioSource && pickupSound)
            audioSource.PlayOneShot(pickupSound);

        GameManager.Instance.SetState(GameState.HasKey);

        // On ne cache plus l'objet — le joueur la tient maintenant en main
    }

    /// <summary>
    /// Retourne true si la clé est en ce moment tenue par un interactor XR.
    /// C'est ce que BasementDoor vérifiera.
    /// </summary>
    public bool IsHeld()
    {
        return grabInteractable != null && grabInteractable.isSelected;
    }

    // Gardé pour compatibilité si d'autres scripts l'utilisent encore
    public bool IsPickedUp() => isPickedUp;
}