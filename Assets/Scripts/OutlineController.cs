using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineController : MonoBehaviour
{
    [Header("Paramètres d'outline")]
    public Color outlineColor = new Color(1f, 0.75f, 0.2f);
    public float pulseSpeed = 1.5f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;

    [Header("État")]
    [SerializeField] private bool outlineEnabled = false;

    private Renderer[] renderers;
    private MaterialPropertyBlock propBlock;
    private Material[] cachedMaterials;
    private bool isHovering = false;
    private float timeOffset;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();
        timeOffset = Random.Range(0f, Mathf.PI * 2f);

        var matList = new List<Material>();
        foreach (var r in renderers)
            foreach (var mat in r.materials)
                matList.Add(mat);
        cachedMaterials = matList.ToArray();

        foreach (var mat in cachedMaterials)
            mat.EnableKeyword("_EMISSION");
    }

    void OnEnable()
    {
        OutlinePulseManager.Register(this);

        if (outlineEnabled)
            SetEmissionDirect(outlineColor * maxAlpha);
        else
            SetEmissionDirect(Color.black);
    }

    void OnDisable()
    {
        OutlinePulseManager.Unregister(this);
        SetEmissionDirect(Color.black);
    }


    /// <summary>
    /// Force l'outline à un état précis.
    /// </summary>
    public void SetOutline(bool active)
    {
        outlineEnabled = active;
        if (active)
            SetEmissionDirect(outlineColor * maxAlpha);
        else
            SetEmissionDirect(Color.black);
    }

    public void Tick(float time)
    {
        if (!outlineEnabled) return;

        if (isHovering)
        {
            SetEmissionDirect(outlineColor * maxAlpha * 1.5f);
            return;
        }

        float t = Mathf.Sin((time + timeOffset) * pulseSpeed) * 0.5f + 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        SetEmissionDirect(outlineColor * alpha);
    }

    public void OnHoverEnter() { isHovering = true; }
    public void OnHoverExit()  { isHovering = false; }

    private void SetEmissionDirect(Color color)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", color);
            r.SetPropertyBlock(propBlock);
        }
    }
}