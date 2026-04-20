using System.Collections;
using UnityEngine;
 
public class PowerOutage : MonoBehaviour
{
    [Header("Lumières de la maison")]
    public Light[] houseLights;          // toutes les lumières normales de la maison
    public Light lightningLight;         // lumière directionnelle pour les éclairs
    public float normalIntensity = 1f;

    [Header("Réglages Orage continu")]
    public bool autoLightning = true;  // Est-ce que les éclairs continuent ?
    public float minInterval = 10f;    // Temps minimum entre deux éclairs
    public float maxInterval = 30f;    // Temps maximum
 
    [Header("Audio")]
    public AudioSource audioSource;     // un seul AudioSource pour tout
    public AudioClip thunderClap;       // le coup de tonnerre (son ponctuel)
    public AudioClip rainAmbience;      // la pluie (loop en arrière-plan)
 
    [Header("Post-Processing (optionnel)")]
    // Si tu utilises le Volume URP, tu peux le référencer ici
    // public Volume postProcessVolume;
 
    [Header("Délai avant coupure")]
    public float delayBeforeOutage = 3.5f;
 
    private bool powerIsOn = true;
 
    void Start()
    {
        lightningLight.intensity = 0f;
        lightningLight.color = new Color(0.9f, 0.95f, 1.0f);
    }
 
    /// <summary>
    /// Appeler cette méthode quand le joueur allume l'interrupteur d'entrée
    /// </summary>
    public void TriggerOutage()
    {
        if (!powerIsOn) return;
        StartCoroutine(OutageSequence());
    }
 
    IEnumerator OutageSequence()
    {
        // Lancer la pluie en loop
        if (audioSource != null && rainAmbience != null)
        {
            audioSource.clip = rainAmbience;
            audioSource.loop = true;
            audioSource.Play();
        }
 
        yield return new WaitForSeconds(delayBeforeOutage);
 
        // Éclairs + flickering
        yield return StartCoroutine(LightningFlash(3));
 
        // Coupure totale
        SetHouseLights(false);
        powerIsOn = false;
 
        // Coup de tonnerre par-dessus la pluie
        if (audioSource != null && thunderClap != null)
            audioSource.PlayOneShot(thunderClap, 1f);
 
        GameManager.Instance.SetState(GameState.PowerOutage);
        StartCoroutine(ContinuousLightning());
    }
 
    IEnumerator LightningFlash(int flashCount)
    {
        for (int i = 0; i < flashCount; i++)
        {
            // Flickering des lumières maison
            float flickerDuration = Random.Range(0.05f, 0.15f);
            SetHouseLightsIntensity(Random.Range(0.1f, 0.5f));
            lightningLight.intensity = Random.Range(2f, 5f);
 
            yield return new WaitForSeconds(flickerDuration);
 
            SetHouseLightsIntensity(normalIntensity);
            lightningLight.intensity = 0f;
 
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }
    }
 
    public void RestorePower()
    {
        StartCoroutine(PowerRestoreSequence());
    }
 
    IEnumerator PowerRestoreSequence()
    {
        // Quelques flickerings avant que ça tienne
        for (int i = 0; i < 4; i++)
        {
            SetHouseLights(true);
            SetHouseLightsIntensity(Random.Range(0.3f, 0.8f));
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            SetHouseLights(false);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));
        }
 
        // Lumière stable
        SetHouseLights(true);
        SetHouseLightsIntensity(normalIntensity);
        powerIsOn = true;
 
        // Stop la pluie progressivement
        if (audioSource != null)
        {
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 0.5f;
                audioSource.volume = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
            audioSource.Stop();
            audioSource.volume = 1f; // reset le volume pour la prochaine fois
        }
    }
 
    void SetHouseLights(bool on)
    {
        foreach (var l in houseLights)
            if (l != null) l.enabled = on;
    }
 
    void SetHouseLightsIntensity(float intensity)
    {
        foreach (var l in houseLights)
            if (l != null) l.intensity = intensity;
    }
    
    IEnumerator ContinuousLightning()
    {
        while (!powerIsOn && autoLightning) // Tant qu'il n'y a pas de courant
        {
            // On attend un temps aléatoire
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            // On déclenche l'éclair (3 flashs comme dans ta séquence)
            yield return StartCoroutine(LightningFlash(Random.Range(1, 4)));

            // On joue le son du tonnerre avec un petit délai (plus réaliste)
            if (audioSource != null && thunderClap != null)
            {
                yield return new WaitForSeconds(0.5f); // Le son arrive après la lumière
                audioSource.PlayOneShot(thunderClap, Random.Range(0.6f, 1f));
            }
        }
    }
}