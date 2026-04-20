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
    public AudioClip extinguishSound;

    [Header("Configuration")]
    public float maxLightIntensity = 1.2f;
    public bool enableRandomExtinguish = true;
    public float minTimeBetweenExtinguish = 20f;
    public float maxTimeBetweenExtinguish = 45f;
 
    private bool isLit = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
 
    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnInteract);
        
        // On s'assure qu'elle est éteinte au départ
        ExtinguishCandle();
    }
 
    void OnInteract(SelectEnterEventArgs args)
    {
        // On ne peut l'allumer que si le courant a sauté
        if (!isLit && GameManager.Instance.currentState == GameState.PowerOutage)
        {
            LightCandle();
        }
    }
 
    public void LightCandle()
    {
        isLit = true;
        if (candleLight != null) candleLight.enabled = true;
        if (flameParticles != null) flameParticles.Play();
        if (audioSource && lightMatchSound) audioSource.PlayOneShot(lightMatchSound);
 
        GameManager.Instance.SetState(GameState.CandleLit);
        
        if (enableRandomExtinguish)
            StartCoroutine(RandomExtinguishRoutine());
    }
 
    public void ExtinguishCandle()
    {
        isLit = false;
        if (candleLight != null) candleLight.enabled = false;
        if (flameParticles != null) flameParticles.Stop();
        if (audioSource && extinguishSound) audioSource.PlayOneShot(extinguishSound);
    }
 
    // VOICI LA FONCTION QUI POSAIT PROBLÈME
    IEnumerator RandomExtinguishRoutine()
    {
        float delay = Random.Range(minTimeBetweenExtinguish, maxTimeBetweenExtinguish);
        yield return new WaitForSeconds(delay);
 
        // On vérifie les états réels de ton GameManager
        if (isLit && (GameManager.Instance.currentState == GameState.CandleLit || 
                      GameManager.Instance.currentState == GameState.SearchingKeyRDC ||
                      GameManager.Instance.currentState == GameState.HasKey))
        {
            ExtinguishCandle();
            GameManager.Instance.dialogueSystem.ShowDialogue("Mince, la bougie s'est éteinte !", 3f, null);
        }
    }
}