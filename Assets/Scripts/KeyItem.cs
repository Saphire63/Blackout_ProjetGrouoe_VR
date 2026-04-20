using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyItem : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip pickupSound;
    private bool isPickedUp = false;

    public bool IsPickedUp() => isPickedUp;

    public void PickUp()
    {
        if (isPickedUp) return;
        isPickedUp = true;
        if (audioSource && pickupSound) audioSource.PlayOneShot(pickupSound);
        GameManager.Instance.SetState(GameState.HasKey);
        GetComponent<MeshRenderer>().enabled = false;
        if(GetComponent<Collider>()) GetComponent<Collider>().enabled = false;
    }
}