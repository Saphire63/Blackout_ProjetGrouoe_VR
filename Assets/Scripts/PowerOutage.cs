using System.Collections;
using UnityEngine;

public class PowerOutage : MonoBehaviour
{
    [Header("Outline ciblé")]
    [SerializeField] private OutlineController targetOutline;

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
    private Coroutine activeSequence;

    void Start()
    {
        SetHouseLights(false, 0f);

        // Précharge les deux clips en mémoire au démarrage
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            if (rainAmbience != null)
            {
                audioSource.clip = rainAmbience;
                rainAmbience.LoadAudioData();
            }
        }
        if (thunderClap != null)
            thunderClap.LoadAudioData();
    }

    public void TriggerOutage()
    {
        if (!powerIsOn || activeSequence != null) return;
        activeSequence = StartCoroutine(OutageSequence());
    }

    public void TurnOnThenOutage()
    {
        if (activeSequence != null) return;
        activeSequence = StartCoroutine(TurnOnThenOutageSequence());
    }

    IEnumerator TurnOnThenOutageSequence()
    {
        SetHouseLights(true, normalIntensity);
        powerIsOn = true;

        yield return new WaitForSeconds(delayBeforeOutage);

        PlayLoopAudio(rainAmbience);

        yield return LightningFlash(3);

        SetHouseLights(false, 0f);
        powerIsOn = false;


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



        activeSequence = null;
        GameManager.Instance.SetState(GameState.PowerOutage);
    }

    IEnumerator LightningFlash(int flashCount)
    {
        for (int i = 0; i < flashCount; i++)
        {
            float flickerIntensity = Random.Range(0.1f, 0.5f);
            SetHouseLights(true, flickerIntensity);
            if (lightningLight)
            {
                lightningLight.enabled = true;
            }
            PlayOneShot(thunderClap, 3f);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

            SetHouseLights(true, normalIntensity);
            if (lightningLight)
                lightningLight.enabled = false;

            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }
        if (targetOutline != null)
        {
            targetOutline.SetOutline(true);
        }

    }

    public void RestorePower()
    {
        if (activeSequence != null) StopCoroutine(activeSequence);
        activeSequence = StartCoroutine(PowerRestoreSequence());
    }

    IEnumerator PowerRestoreSequence()
    {
        for (int i = 0; i < 4; i++)
        {
            SetHouseLights(true, Random.Range(0.3f, 0.8f));
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            SetHouseLights(false, 0f);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));
        }

        SetHouseLights(true, normalIntensity);
        powerIsOn = true;

        // Fade out audio
        if (audioSource != null && audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;
            float elapsed = 0f;
            float fadeDuration = 2f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = 1f;
        }

        activeSequence = null;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

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
        if (audioSource == null || clip == null) return;
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }
}