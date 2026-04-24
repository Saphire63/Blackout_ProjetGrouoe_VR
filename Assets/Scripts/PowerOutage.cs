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

    private bool powerIsOn = false; // false au départ — lumières éteintes

    void Start()
    {
        // Lumières éteintes dès le lancement
        if (lightningLight) lightningLight.intensity = 0f;
        SetHouseLights(false);
    }

    public void TriggerOutage()
    {
        if (powerIsOn) StartCoroutine(OutageSequence());
    }

    // Appelé par LightSwitch quand le joueur allume l'interrupteur
    public void TurnOnThenOutage()
    {
        StartCoroutine(TurnOnThenOutageSequence());
    }

    IEnumerator TurnOnThenOutageSequence()
    {
        // Allumer brièvement
        SetHouseLights(true);
        powerIsOn = true;
        yield return new WaitForSeconds(delayBeforeOutage);

        // Lancer la pluie
        if (audioSource != null && rainAmbience != null)
        {
            audioSource.clip = rainAmbience;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Éclairs
        yield return StartCoroutine(LightningFlash(3));

        // Coupure
        SetHouseLights(false);
        powerIsOn = false;

        // Tonnerre
        if (audioSource != null && thunderClap != null)
            audioSource.PlayOneShot(thunderClap, 1f);

        // Notifier GameManager
        GameManager.Instance.SetState(GameState.PowerOutage);
    }

    IEnumerator OutageSequence()
    {
        if (audioSource != null && rainAmbience != null)
        {
            audioSource.clip = rainAmbience;
            audioSource.loop = true;
            audioSource.Play();
        }

        yield return new WaitForSeconds(delayBeforeOutage);
        yield return StartCoroutine(LightningFlash(3));

        SetHouseLights(false);
        powerIsOn = false;

        if (audioSource != null && thunderClap != null)
            audioSource.PlayOneShot(thunderClap, 1f);

        GameManager.Instance.SetState(GameState.PowerOutage);
    }

    IEnumerator LightningFlash(int flashCount)
    {
        for (int i = 0; i < flashCount; i++)
        {
            SetHouseLightsIntensity(Random.Range(0.1f, 0.5f));
            if (lightningLight) lightningLight.intensity = Random.Range(2f, 5f);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

            SetHouseLightsIntensity(normalIntensity);
            if (lightningLight) lightningLight.intensity = 0f;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }
    }

    public void RestorePower()
    {
        StartCoroutine(PowerRestoreSequence());
    }

    IEnumerator PowerRestoreSequence()
    {
        for (int i = 0; i < 4; i++)
        {
            SetHouseLights(true);
            SetHouseLightsIntensity(Random.Range(0.3f, 0.8f));
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            SetHouseLights(false);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));
        }

        SetHouseLights(true);
        SetHouseLightsIntensity(normalIntensity);
        powerIsOn = true;

        // Arrêter la pluie
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
            audioSource.volume = 1f;
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
}