using System.Collections;
using UnityEngine;

public class PowerOutage : MonoBehaviour
{
    [Header("Lumières")]
    public Light[] houseLights; 

    [Header("Sons d'ambiance")]
    public AudioSource audioSource; // L'AudioSource sur cet objet
    public AudioClip thunderClip;  // Le son du tonnerre (un coup)
    public AudioClip rainClip;     // Le son de la pluie (en boucle)

    public void TriggerOutage()
    {
        StartCoroutine(OutageRoutine());
    }

    IEnumerator OutageRoutine()
    {
        // 1. On allume tout au début pendant 1 seconde
        SetLights(true);
        yield return new WaitForSeconds(1.0f);

        // 2. LA COUPURE
        SetLights(false);

        // 3. LE TONNERRE (Le gros "Boom")
        if (audioSource != null && thunderClip != null)
        {
            audioSource.PlayOneShot(thunderClip);
        }

        // 4. LA PLUIE (On change le son de l'AudioSource pour mettre la pluie en boucle)
        if (audioSource != null && rainClip != null)
        {
            audioSource.clip = rainClip;
            audioSource.loop = true; // Pour que la pluie ne s'arrête jamais
            audioSource.Play();
        }

        // 5. CHANGEMENT D'ÉTAT ET DIALOGUE
        GameManager.Instance.SetState(GameState.PowerOutage);
        GameManager.Instance.dialogueSystem.ShowDialogue("Mince, le courant ! Je dois trouver une bougie.", 5f, null);
    }

    void SetLights(bool state)
    {
        foreach (Light l in houseLights)
        {
            if (l != null) l.enabled = state;
        }
    }

    // Fonction pour rallumer à la fin du jeu
    public void RestorePower()
    {
        SetLights(true);
        if (audioSource != null) audioSource.Stop(); // On arrête la pluie
    }
}