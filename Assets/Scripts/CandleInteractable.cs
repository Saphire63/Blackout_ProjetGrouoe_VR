using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CandleInteractable : MonoBehaviour
{
    [Header("Références")]
    public Light candleLight;
    public ParticleSystem flameParticles;
    public AudioSource audioSource;
    public AudioClip lightMatchSound;
    public AudioClip flameBurnLoop;
    public AudioClip extinguishSound;

    [Header("Lumière")]
    public float maxLightIntensity = 1.2f;
    public float lightFlickerSpeed = 8f;
    public float lightFlickerAmount = 0.15f;
    public Color candleLightColor = new Color(1f, 0.6f, 0.2f);

    [Header("Extinction aléatoire")]
    public bool enableRandomExtinguish = true;
    public float minTimeBetweenExtinguish = 20f;
    public float maxTimeBetweenExtinguish = 45f;

    private bool isLit = false;
    private Coroutine flickerCoroutine;
    private Coroutine extinguishCoroutine;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable rayInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rayInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        if (rayInteractable == null)
            rayInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        rayInteractable.selectEntered.AddListener(OnRaySelect);

        if (candleLight) candleLight.enabled = false;
        if (flameParticles) flameParticles.Stop();
    }

    void OnRaySelect(SelectEnterEventArgs args)
    {
        if (!isLit) LightCandle();
    }

    public void LightCandle()
    {
        if (isLit) return;
        isLit = true;

        if (audioSource && lightMatchSound) audioSource.PlayOneShot(lightMatchSound);
        if (flameParticles) flameParticles.Play();

        if (candleLight)
        {
            candleLight.enabled = true;
            candleLight.color = candleLightColor;
            candleLight.intensity = maxLightIntensity;
        }

        if (audioSource && flameBurnLoop)
        {
            audioSource.clip = flameBurnLoop;
            audioSource.loop = true;
            audioSource.Play();
        }

        flickerCoroutine = StartCoroutine(FlickerLight());

        if (enableRandomExtinguish)
        {
            if (extinguishCoroutine != null) StopCoroutine(extinguishCoroutine);
            extinguishCoroutine = StartCoroutine(RandomExtinguishRoutine());
        }

        if (GameManager.Instance.currentState == GameState.PowerOutage)
            GameManager.Instance.SetState(GameState.CandleLit);
    }

    public void ExtinguishCandle()
    {
        if (!isLit) return;
        isLit = false;

        if (flickerCoroutine != null) { StopCoroutine(flickerCoroutine); flickerCoroutine = null; }

        if (audioSource && extinguishSound)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(extinguishSound);
        }

        StartCoroutine(ExtinguishRoutine());
    }

    IEnumerator ExtinguishRoutine()
    {
        float t = 0f;
        float startIntensity = candleLight ? candleLight.intensity : 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            if (candleLight) candleLight.intensity = Mathf.Lerp(startIntensity, 0f, t);
            yield return null;
        }

        if (candleLight) candleLight.enabled = false;
        if (flameParticles) flameParticles.Stop();

        GameManager.Instance.dialogueSystem.ShowDialogue(
            "La bougie... je dois la rallumer.", 3f, null);
    }

    IEnumerator RandomExtinguishRoutine()
    {
        float delay = Random.Range(minTimeBetweenExtinguish, maxTimeBetweenExtinguish);
        yield return new WaitForSeconds(delay);

        GameState s = GameManager.Instance.currentState;
        if (isLit && (s == GameState.CandleLit || s == GameState.SearchingKeyRDC || s == GameState.SearchingKeyUpstairs))
            ExtinguishCandle();
    }

    IEnumerator FlickerLight()
    {
        while (isLit && candleLight != null)
        {
            float noise = Mathf.PerlinNoise(Time.time * lightFlickerSpeed, 0f);
            float targetIntensity = maxLightIntensity + (noise - 0.5f) * lightFlickerAmount * 2f;
            candleLight.intensity = Mathf.Max(0, targetIntensity);
            yield return null;
        }
    }

    public bool IsLit() => isLit;
}