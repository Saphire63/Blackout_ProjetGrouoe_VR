using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyItem : MonoBehaviour
{
    [Header("Références")]
    public OutlineController outlineController;

    [Header("Effets au ramassage")]
    public AudioSource audioSource;
    public AudioClip pickupSound;           // son de ramassage (tintement de clé)

    private bool isPickedUp = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;  // ray uniquement, pas de grab

    void Awake()
    {
        // On utilise XRSimpleInteractable au lieu de XRGrabInteractable
        // pour que la clé disparaisse au lieu d'être tenue physiquement
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        interactable.selectEntered.AddListener(OnRaySelect);
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);

        outlineController = GetComponent<OutlineController>();
    }

    void OnRaySelect(SelectEnterEventArgs args)
    {
        if (isPickedUp) return;
        PickUp();
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        outlineController?.OnHoverEnter();
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        outlineController?.OnHoverExit();
    }

    void PickUp()
    {
        isPickedUp = true;

        // Désactiver l'outline
        if (outlineController) outlineController.SetOutline(false);

        // Son de ramassage
        if (audioSource && pickupSound)
            audioSource.PlayOneShot(pickupSound);

        // Notifier le GameManager
        GameManager.Instance.SetState(GameState.HasKey);

        // Faire disparaître la clé avec une petite animation
        StartCoroutine(PickUpAnimation());
    }

    IEnumerator PickUpAnimation()
    {
        // Monter légèrement et rétrécir avant de disparaître
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            transform.position = Vector3.Lerp(startPos, startPos + Vector3.up * 0.1f, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public bool IsPickedUp() => isPickedUp;
}