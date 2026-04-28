using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BasementDoor : MonoBehaviour
{
    public KeyItem requiredKey;
    public DoorInteractable doorHandle;
    public AudioSource doorAudio;
    public AudioClip lockedSound;
    public AudioClip unlockSound;

    private bool isLocked = true;

    void Awake()
    {
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnInteract);
    }

    void OnInteract(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if (!isLocked) return;

        // Le joueur DOIT tenir la clé en main au moment d'interagir avec la porte
        if (requiredKey != null && requiredKey.IsHeld())
        {
            Unlock();
        }
        else if (requiredKey != null && requiredKey.IsPickedUp())
        {
            // Le joueur a la clé quelque part mais ne la tient pas en main
            if (doorAudio && lockedSound) doorAudio.PlayOneShot(lockedSound);
            GameManager.Instance.dialogueSystem.ShowDialogue(
                "J'ai la clé... il faut que j'aille ouvrir la porte.", 3f, null);
        }
        else
        {
            // Le joueur n'a pas encore trouvé la clé
            if (doorAudio && lockedSound) doorAudio.PlayOneShot(lockedSound);
            GameManager.Instance.dialogueSystem.ShowDialogue(
                "C'est fermé... il me faut la clé du sous-sol.", 3f, null);

            if (GameManager.Instance.currentState == GameState.CandleLit)
                GameManager.Instance.SetState(GameState.SearchingKeyRDC);
        }
    }

    void Unlock()
    {
        isLocked = false;
        if (doorAudio && unlockSound) doorAudio.PlayOneShot(unlockSound);
        if (doorHandle != null) doorHandle.isLocked = false;

        GameManager.Instance.SetState(GameState.BasementOpen);
        GameManager.Instance.dialogueSystem.ShowDialogue("La serrure est ouverte !", 3f, null);
        GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().enabled = false;
    }
}