using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorInteractable : MonoBehaviour
{
    [Header("Configuration Porte")]
    public Transform doorPivot;
    public float openAngle = 90f;
    public float smoothTime = 2f;
    
    [Header("Verrouillage")]
    public bool isLocked = true; // Coché par défaut pour la cave
    public AudioSource audioSource;
    public AudioClip lockedSound;
    public AudioClip openSound;

    [Header("Dialogue première ouverture (optionnel)")]
    public string firstOpenDialogue = "";
    public float firstOpenDialogueDuration = 3f;

    private bool isOpen = false;
    private bool firstOpenDialoguePlayed = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine moveCoroutine;

    void Start()
    {
        if (doorPivot == null) doorPivot = transform;
        closedRotation = doorPivot.localRotation;
        openRotation = Quaternion.Euler(doorPivot.localEulerAngles + new Vector3(0, openAngle, 0));

        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnInteract);
        }
    }
    
    private void OnInteract(SelectEnterEventArgs args)
    {
        if (isLocked)
        {
            if (audioSource && lockedSound) audioSource.PlayOneShot(lockedSound);
            
            if (GameManager.Instance != null && GameManager.Instance.dialogueSystem != null)
            {
                GameManager.Instance.dialogueSystem.ShowDialogue("C'est fermé à clé... Je devrais examiner la serrure.", 3f, null);
            }
        }
        else
        {
            if (audioSource && openSound) audioSource.PlayOneShot(openSound);

            if (!firstOpenDialoguePlayed && !string.IsNullOrEmpty(firstOpenDialogue))
            {
                firstOpenDialoguePlayed = true;
                GameManager.Instance.dialogueSystem.ShowDialogue(firstOpenDialogue, firstOpenDialogueDuration, null);
            }

            ToggleDoor();
        }
    }

    public void ToggleDoor()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        isOpen = !isOpen;
        moveCoroutine = StartCoroutine(MoveDoor(isOpen ? openRotation : closedRotation));
    }

    IEnumerator MoveDoor(Quaternion targetRotation)
    {
        float elapsed = 0;
        Quaternion startRotation = doorPivot.localRotation;
        while (elapsed < 1f)
        {
            doorPivot.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsed);
            elapsed += Time.deltaTime * smoothTime;
            yield return null;
        }
        doorPivot.localRotation = targetRotation;
    }
}