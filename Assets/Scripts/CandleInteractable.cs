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
    public AudioClip extinguishSound;       // son de bougie qui s'éteint (souffle)
 
    [Header("Lumière de la bougie")]
    public float maxLightIntensity = 1.2f;
    public float lightFlickerSpeed = 8f;
    public float lightFlickerAmount = 0.15f;
    public Color candleLightColor = new Color(1f, 0.6f, 0.2f);
 
    [Header("Extinction aléatoire")]
    public bool enableRandomExtinguish = true;
    [Tooltip("Délai minimum en secondes avant une extinction aléatoire")]
    public float minTimeBetweenExtinguish = 20f;
    [Tooltip("Délai maximum en secondes avant une extinction aléatoire")]
    public float maxTimeBetweenExtinguish = 45f;
 
    [Header("Événements ambiants")]
    [Tooltip("Le script AmbientEvent à déclencher quand la bougie s'éteint")]
    public AmbientEvent ambientEvent;
 
    // ─── Privé ──────────────────────────────────────────────
    private bool isLit = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable rayInteractable;   // pour le ray (pointer + trigger)
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;    // pour ramasser
    private Coroutine flickerCoroutine;
    private Coroutine extinguishCoroutine;
 
    void Awake()
    {
        // Grab pour ramasser la bougie
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
 
        // Simple interactable pour le ray (rallumer)
        rayInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (rayInteractable == null)
            rayInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
 
        rayInteractable.selectEntered.AddListener(OnRaySelect);
 
        // Désactivé au départ
        if (candleLight) candleLight.enabled = false;
        if (flameParticles) flameParticles.Stop();
    }
 
    // ─── Interaction Ray (rallumer) ──────────────────────────
 
    void OnRaySelect(SelectEnterEventArgs args)
    {
        // Si la bougie est éteinte → la rallumer
        if (!isLit)
            LightCandle();
    }
 
    // ─── Allumage ────────────────────────────────────────────
 
    public void LightCandle()
    {
        if (isLit) return;
        isLit = true;
 
        if (audioSource && lightMatchSound)
            audioSource.PlayOneShot(lightMatchSound);
 
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
 
        // Programmer la prochaine extinction aléatoire
        if (enableRandomExtinguish)
        {
            if (extinguishCoroutine != null) StopCoroutine(extinguishCoroutine);
            extinguishCoroutine = StartCoroutine(RandomExtinguishRoutine());
        }
 
        // Notifier le GameManager seulement la première fois
        if (GameManager.Instance.currentState == GameState.PowerOutage)
            GameManager.Instance.SetState(GameState.CandleLit);
    }
 
    // ─── Extinction ──────────────────────────────────────────
 
    public void ExtinguishCandle()
    {
        if (!isLit) return;
        isLit = false;
 
        // Arrêter le flickering
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }
 
        // Son d'extinction
        if (audioSource && extinguishSound)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(extinguishSound);
        }
 
        // Éteindre flamme et lumière progressivement
        StartCoroutine(ExtinguishRoutine());
    }
 
    IEnumerator ExtinguishRoutine()
    {
        // Fade out de la lumière
        float t = 0f;
        float startIntensity = candleLight ? candleLight.intensity : 0f;
 
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            if (candleLight)
                candleLight.intensity = Mathf.Lerp(startIntensity, 0f, t);
            yield return null;
        }
 
        if (candleLight) candleLight.enabled = false;
        if (flameParticles) flameParticles.Stop();
 
        // Déclencher un événement ambiant
        if (ambientEvent != null)
            ambientEvent.TriggerRandomEvent();
 
        // Dialogue
        GameManager.Instance.dialogueSystem.ShowDialogue(
            "La bougie... je dois la rallumer.", 3f, null);
    }
 
    // ─── Extinction aléatoire ────────────────────────────────
 
    IEnumerator RandomExtinguishRoutine()
    {
        // Attendre un délai aléatoire
        float delay = Random.Range(minTimeBetweenExtinguish, maxTimeBetweenExtinguish);
        yield return new WaitForSeconds(delay);
 
        // N'éteindre que si le joueur est encore dans la phase "bougie allumée"
        // (pas si le courant est déjà rétabli)
        if (isLit && GameManager.Instance.currentState == GameState.CandleLit
            || GameManager.Instance.currentState == GameState.SearchingKeyRDC
            || GameManager.Instance.currentState == GameState.SearchingKeyUpstairs)
        {
            ExtinguishCandle();
        }
    }
 
    // ─── Flickering ─────────────────────────────────────────
 
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
 