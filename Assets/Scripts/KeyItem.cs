using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyItem : MonoBehaviour
{
    public AudioSource audioS;
    public AudioClip pickupSound;
    private bool picked = false;

    public bool IsPickedUp() => picked;

    // FONCTION À LIER DANS L'INSPECTEUR (Select Entered)
    public void OnGrabbed()
    {
        if (picked) return;
        picked = true;

        if (audioS && pickupSound) audioS.PlayOneShot(pickupSound);

        GameManager.Instance.SetState(GameState.HasKey);
        GameManager.Instance.dialogueSystem.ShowDialogue("C'est bon, j'ai la clé de la cave !", 3f, null);
        
        this.gameObject.SetActive(false); // Elle disparait
    }
}