using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Porte à charnières — interaction via Ray Interactor (pointer + trigger).
/// Le joueur n'a pas besoin de s'approcher physiquement.
///
/// SETUP dans Unity :
///  DoorPivot (Transform côté charnières)
///  └── DoorMesh (le mesh visible)
///      └── Handle (GameObject avec XRSimpleInteractable + ce script)
/// </summary>
public class DoorInteractable : MonoBehaviour
{
    [Header("Pivot de rotation")]
    [Tooltip("Le GameObject côté charnières — c'est lui qui tourne, pas le mesh direct.")]
    public Transform doorPivot;

    [Header("Ouverture")]
    public float openAngle = 90f;
    [Range(0.5f, 5f)]
    public float openSpeed = 2.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;

    [Header("Verrouillage")]
    public bool isLocked = false;

    // ─── Privé ──────────────────────────────────────────────
    private bool isOpen = false;
    private bool isAnimating = false;
    private float currentAngle = 0f;
    private float cachedOpenDirection = 1f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private Transform playerCamera;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        interactable.selectEntered.AddListener(OnRaySelect);
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);

        playerCamera = Camera.main?.transform;
    }

    // ─── Events Ray Interactor ───────────────────────────────

    void OnRaySelect(SelectEnterEventArgs args)
    {
        if (isAnimating) return;

        if (isLocked)
        {
            PlaySound(lockedSound);
            return;
        }

        if (isOpen) CloseDoor();
        else OpenDoor();
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        // Surbrillance au hover du rayon
        var outline = GetComponentInParent<OutlineController>();
        outline?.OnHoverEnter();
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        var outline = GetComponentInParent<OutlineController>();
        outline?.OnHoverExit();
    }

    // ─── Ouverture / Fermeture ───────────────────────────────

    public void OpenDoor()
    {
        if (isOpen || isAnimating || doorPivot == null) return;
        isOpen = true;

        // Détecter de quel côté est le joueur pour ouvrir vers lui
        if (playerCamera != null)
        {
            Vector3 toPlayer = playerCamera.position - doorPivot.position;
            float dot = Vector3.Dot(doorPivot.right, toPlayer);
            cachedOpenDirection = dot >= 0f ? 1f : -1f;
        }

        PlaySound(openSound);
        StartCoroutine(RotateDoor(openAngle * cachedOpenDirection));
    }

    public void CloseDoor()
    {
        if (!isOpen || isAnimating || doorPivot == null) return;
        isOpen = false;

        PlaySound(closeSound);
        StartCoroutine(RotateDoor(0f));
    }

    IEnumerator RotateDoor(float targetAngle)
    {
        isAnimating = true;
        float startAngle = currentAngle;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            currentAngle = Mathf.Lerp(startAngle, targetAngle, EaseInOut(t));
            doorPivot.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
            yield return null;
        }

        currentAngle = targetAngle;
        doorPivot.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
        isAnimating = false;
    }

    float EaseInOut(float t)
    {
        t = Mathf.Clamp01(t);
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource && clip) audioSource.PlayOneShot(clip);
    }

    // ─── API publique ────────────────────────────────────────
    public void SetLocked(bool locked) => isLocked = locked;
    public bool IsOpen() => isOpen;
    public bool IsLocked() => isLocked;
}
