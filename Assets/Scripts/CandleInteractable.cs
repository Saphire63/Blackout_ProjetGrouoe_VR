using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CandleInteractable : MonoBehaviour
{
    public Light candleLight;
    public ParticleSystem flameParticles;
    public AudioSource audioSource;
    public AudioClip lightMatchSound;
    private bool isLit = false;

    void Awake()
    {
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener((args) => {
            if (!isLit && GameManager.Instance.currentState == GameState.PowerOutage)
                LightCandle();
        });
        if (candleLight) candleLight.enabled = false;
        if (flameParticles) flameParticles.Stop();
    }

    public void LightCandle()
    {
        isLit = true;
        if (candleLight) candleLight.enabled = true;
        if (flameParticles) flameParticles.Play();
        if (audioSource && lightMatchSound) audioSource.PlayOneShot(lightMatchSound);

        GameManager.Instance.SetState(GameState.CandleLit);
    }
}