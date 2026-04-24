using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyItem : MonoBehaviour
{
    [Header("Effets")]
    public AudioSource audioSource;
    public AudioClip pickupSound;

    private bool isPickedUp = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        interactable.selectEntered.AddListener(OnRaySelect);
    }

    void OnRaySelect(SelectEnterEventArgs args)
    {
        if (isPickedUp) return;
        RecupererCle();
    }

    public void RecupererCle()
    {
        if (isPickedUp) return;
        isPickedUp = true;

        if (audioSource && pickupSound)
            audioSource.PlayOneShot(pickupSound);

        GameManager.Instance.SetState(GameState.HasKey);

        // Animation : monte légèrement et rétrécit avant de disparaître
        StartCoroutine(PickUpAnimation());
    }

    IEnumerator PickUpAnimation()
    {
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