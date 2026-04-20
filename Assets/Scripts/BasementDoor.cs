using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BasementDoor : MonoBehaviour
{
    public KeyItem requiredKey;
    public DoorInteractable doorHandle; // Glisse la poignée ici dans l'inspecteur
    public AudioSource doorAudio;
    public AudioClip lockedSound;
    public AudioClip unlockSound;

    private bool isLocked = true;

    void Awake() {
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnInteract);
    }

    void OnInteract(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args) {
        if (!isLocked) return;

        if (requiredKey != null && requiredKey.IsPickedUp()) {
            Unlock();
        } else {
            if (doorAudio && lockedSound) doorAudio.PlayOneShot(lockedSound);
            GameManager.Instance.dialogueSystem.ShowDialogue("C'est fermé... il me faut la clé du sous-sol.", 3f, null);
            if (GameManager.Instance.currentState == GameState.CandleLit)
                GameManager.Instance.SetState(GameState.SearchingKeyRDC);
        }
    }

    void Unlock() {
        isLocked = false;
        if (doorAudio && unlockSound) doorAudio.PlayOneShot(unlockSound);
        if (doorHandle != null) doorHandle.isLocked = false; // DÉBLOQUE LA POIGNÉE

        GameManager.Instance.SetState(GameState.BasementOpen);
        GameManager.Instance.dialogueSystem.ShowDialogue("La serrure est ouverte !", 3f, null);
        GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().enabled = false;
    }
}