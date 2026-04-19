using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Porte coulissante — interaction via Ray Interactor.
/// Utile pour cave, placard, entrée style moderne.
///
/// SETUP dans Unity :
///  SlidingDoorRoot (ce script)
///  ├── DoorMesh
///  └── Handle (XRSimpleInteractable)
/// </summary>
public class SlidingDoor : MonoBehaviour
{
    [Header("Transform de la porte")]
    [Tooltip("Le Transform qui glisse. Si vide, utilise ce GameObject.")]
    public Transform doorTransform;

    [Header("Mouvement")]
    public Vector3 slideDirection = Vector3.right;
    public float slideDistance = 1f;
    [Range(0.5f, 5f)]
    public float slideSpeed = 2.5f;

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
    private Vector3 closedLocalPos;
    private Vector3 openLocalPos;

    void Awake()
    {
        if (doorTransform == null) doorTransform = transform;

        closedLocalPos = doorTransform.localPosition;
        openLocalPos = closedLocalPos + slideDirection.normalized * slideDistance;

        var handle = GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (handle != null)
        {
            handle.selectEntered.AddListener(OnRaySelect);
            handle.hoverEntered.AddListener(OnHoverEnter);
            handle.hoverExited.AddListener(OnHoverExit);
        }
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
        var outline = GetComponentInChildren<OutlineController>();
        outline?.OnHoverEnter();
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        var outline = GetComponentInChildren<OutlineController>();
        outline?.OnHoverExit();
    }

    // ─── Ouverture / Fermeture ───────────────────────────────

    public void OpenDoor()
    {
        if (isOpen || isAnimating) return;
        isOpen = true;
        PlaySound(openSound);
        StartCoroutine(SlideRoutine(openLocalPos));
    }

    public void CloseDoor()
    {
        if (!isOpen || isAnimating) return;
        isOpen = false;
        PlaySound(closeSound);
        StartCoroutine(SlideRoutine(closedLocalPos));
    }

    IEnumerator SlideRoutine(Vector3 targetPos)
    {
        isAnimating = true;
        Vector3 startPos = doorTransform.localPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * slideSpeed;
            doorTransform.localPosition = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        doorTransform.localPosition = targetPos;
        isAnimating = false;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource && clip) audioSource.PlayOneShot(clip);
    }

    // ─── API publique ────────────────────────────────────────
    public void Unlock() => isLocked = false;
    public void Lock() => isLocked = true;
    public bool IsOpen() => isOpen;
}
