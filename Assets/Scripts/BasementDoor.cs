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
            OpenDoor();
        }
    }

    void Unlock()
    {
        isLocked = false;
        PlaySound(unlockSound);
        GameManager.Instance.SetState(GameState.BasementOpen);

        // Ouvrir directement après déverrouillage
        StartCoroutine(OpenAfterDelay(0.5f));
    }

    IEnumerator OpenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OpenDoor();
    }

    void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;

        PlaySound(openSound);
        if (doorAnimator) doorAnimator.SetTrigger("Open");
    }

    void PlaySound(AudioClip clip)
    {
        if (doorAudio && clip) doorAudio.PlayOneShot(clip);
    }
}
