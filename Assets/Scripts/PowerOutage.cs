using System.Collections;
using UnityEngine;

public class PowerOutage : MonoBehaviour
{
    [Header("Lumières de la maison")]
    public Light[] houseLights;
    public Light lightningLight;
    public float normalIntensity = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip thunderClap;
    public AudioClip rainAmbience;

    [Header("Délai avant coupure")]
    public float delayBeforeOutage = 3.5f;

    private bool powerIsOn = false;
    private Coroutine activeSequence; // ← garde trace de la séquence en cours

    void Start()
    {
        
        SetHouseLights(false, 0f);
    }

    public void TriggerOutage()
    {
        if (!powerIsOn || activeSequence != null) return; // ← guard
        activeSequence = StartCoroutine(OutageSequence());
    }

    public void TurnOnThenOutage()
    {
        if (activeSequence != null) return; // ← guard anti-doublon
        activeSequence = StartCoroutine(TurnOnThenOutageSequence());
    }

    IEnumerator TurnOnThenOutageSequence()
    {
        SetHouseLights(true, normalIntensity);
        powerIsOn = true;

        yield return new WaitForSeconds(delayBeforeOutage);

        PlayLoopAudio(rainAmbience);

        // LightningFlash sans StartCoroutine imbriqué — on yield directement
        yield return LightningFlash(3);

        SetHouseLights(false, 0f);
        powerIsOn = false;

        PlayOneShot(thunderClap);

        activeSequence = null;
        GameManager.Instance.SetState(GameState.PowerOutage);
    }

    IEnumerator OutageSequence()
    {
        PlayLoopAudio(rainAmbience);

        yield return new WaitForSeconds(delayBeforeOutage);
        yield return LightningFlash(3);

        SetHouseLights(false, 0f);
        powerIsOn = false;

        PlayOneShot(thunderClap);

        activeSequence = null;
        GameManager.Instance.SetState(GameState.PowerOutage);
    }

    IEnumerator LightningFlash(int flashCount)
    {
        for (int i = 0; i < flashCount; i++)
        {
            float flickerIntensity = Random.Range(0.1f, 0.5f); // ← décommente ça
            SetHouseLights(true, flickerIntensity);
            if (lightningLight){
                lightningLight.enabled = true;
            }
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f)); // ← et ça

            SetHouseLights(true, normalIntensity);
            if (lightningLight){ 
                lightningLight.enabled = false;
            }

            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }
        yield break;
    }

    public void RestorePower()
    {
        if (activeSequence != null) StopCoroutine(activeSequence);
        activeSequence = StartCoroutine(PowerRestoreSequence());
    }

    IEnumerator PowerRestoreSequence()
    {
        // Scintillement de rétablissement
        for (int i = 0; i < 4; i++)
        {
            SetHouseLights(true, Random.Range(0.3f, 0.8f));
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            SetHouseLights(false, 0f);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));
        }

        SetHouseLights(true, normalIntensity);
        powerIsOn = true;

        // // Fade out audio — en une seule boucle propre
        // if (audioSource != null && audioSource.isPlaying)
        // {
        //     float startVolume = audioSource.volume;
        //     float elapsed = 0f;
        //     float fadeDuration = 2f;

        //     while (elapsed < fadeDuration)
        //     {
        //         elapsed += Time.deltaTime;
        //         audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
        //         yield return null;
        //     }

        //     audioSource.Stop();
        //     audioSource.volume = 1f;
        // }

        activeSequence = null;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    // Un seul foreach au lieu de deux
    private void SetHouseLights(bool on, float intensity)
    {
        foreach (var l in houseLights)
        {
            if (l == null) continue;
            l.enabled = on;
            l.intensity = intensity;
        }
    }

    private void PlayLoopAudio(AudioClip clip)
    {
        // if (audioSource == null || clip == null) return;
        // audioSource.clip = clip;
        // audioSource.loop = true;
        // audioSource.Play();
    }

    private void PlayOneShot(AudioClip clip)
    {
        // if (audioSource == null || clip == null) return;
        // audioSource.PlayOneShot(clip, 1f);
    }
}