using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BasementDoor : MonoBehaviour
{
    [Header("Animation")]
    public Animator doorAnimator;           // Animator avec clip "Open"
    public AudioSource doorAudio;
    public AudioClip lockedSound;           // son de porte bloquée
    public AudioClip unlockSound;           // son de déverrouillage
    public AudioClip openSound;             // son d'ouverture

    [Header("Référence à la clé")]
    public KeyItem requiredKey;

    [Header("Référence à la poignée")]
    [Tooltip("Le DoorInteractable sur la poignée — sera déverrouillé quand la clé est utilisée")]
    public DoorInteractable doorHandle;

    private bool isLocked = true;
    private bool isOpen = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnInteract);
    }

    void OnInteract(SelectEnterEventArgs args)
    {
        if (isOpen) return;

        if (isLocked)
        {
            if (requiredKey != null && requiredKey.IsPickedUp())
            {
                Unlock();
            }
            else
            {
                // Porte fermée, pas de clé
                PlaySound(lockedSound);

                // Premier contact sans clé → lancer la recherche
                if (GameManager.Instance.currentState == GameState.CandleLit)
                    GameManager.Instance.SetState(GameState.SearchingKeyRDC);
            }
        }
        else
        {
        }
    }

    void Unlock()
    {
        isLocked = false;
        PlaySound(unlockSound);

        // Déverrouiller la poignée
        if (doorHandle != null)
            doorHandle.SetLocked(false);

        GameManager.Instance.SetState(GameState.BasementOpen);

        // Dialogue
        GameManager.Instance.dialogueSystem.ShowDialogue(
            "La serrure s'ouvre... je peux y aller.", 2f, null);
    }

    void PlaySound(AudioClip clip)
    {
        if (doorAudio && clip) doorAudio.PlayOneShot(clip);
    }
}