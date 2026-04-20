using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyItem : MonoBehaviour
{
    [Header("Effets")]
    public AudioSource audioSource;
    public AudioClip pickupSound;

    private bool isPickedUp = false;

    // Cette fonction sera appelée quand tu cliques sur la clé
    public void RecupererCle()
    {
        if (isPickedUp) return;
        isPickedUp = true;

        // 1. Jouer le son
        if (audioSource && pickupSound)
            audioSource.PlayOneShot(pickupSound);

        // 2. Changer l'état du jeu pour débloquer la porte
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.HasKey);
        }

        // 3. Faire disparaître la clé immédiatement
        gameObject.SetActive(false);
        
        Debug.Log("Clé récupérée avec succès !");
    }

    // Garder cette fonction pour la compatibilité avec la porte
    public bool IsPickedUp()
    {
        return isPickedUp;
    }
}