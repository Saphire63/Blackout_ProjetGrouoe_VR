using System.Collections;
using UnityEngine;

/// <summary>
/// Ajoute un outline pulsant sur l'objet pour indiquer qu'il est interactable.
/// Nécessite le package "Quick Outline" (gratuit sur GitHub) ou le Renderer Feature URP custom.
/// Alternative simple : on change l'émission du matériau.
/// </summary>
public class OutlineController : MonoBehaviour
{
    [Header("Paramètres d'outline")]
    public Color outlineColor = new Color(1f, 0.75f, 0.2f); // orange chaud
    public float outlineWidth = 5f;
    public float pulseSpeed = 1.5f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;

    // Si tu utilises Quick Outline asset :
    // private Outline outlineComponent;

    // Solution sans asset externe : émission sur le matériau
    private Renderer[] renderers;
    private bool isOutlineActive = false;
    private Coroutine pulseCoroutine;
    private MaterialPropertyBlock propBlock;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    public void SetOutline(bool active)
    {
        isOutlineActive = active;

        if (active)
        {
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(PulseOutline());
        }
        else
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
            SetEmission(Color.black);
        }
    }

    IEnumerator PulseOutline()
    {
        while (isOutlineActive)
        {
            float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            Color emissionColor = outlineColor * alpha;
            SetEmission(emissionColor);
            yield return null;
        }
    }

    void SetEmission(Color color)
    {
        foreach (var r in renderers)
        {
            r.GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", color);
            r.SetPropertyBlock(propBlock);

            // S'assurer que l'émission est activée sur le matériau
            foreach (var mat in r.materials)
                mat.EnableKeyword("_EMISSION");
        }
    }

    // Appelé quand le joueur s'approche (via XR Interaction Toolkit hover)
    public void OnHoverEnter()
    {
        SetEmission(outlineColor * maxAlpha * 1.5f); // plus brillant au hover
    }

    public void OnHoverExit()
    {
        // Reprend la pulsation normale
        if (isOutlineActive && pulseCoroutine == null)
            pulseCoroutine = StartCoroutine(PulseOutline());
    }
}
