using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OutlineController optimisé — cache les matériaux, mutualise le pulse via un manager statique.
/// </summary>
public class OutlineController : MonoBehaviour
{
    [Header("Paramètres d'outline")]
    public Color outlineColor = new Color(1f, 0.75f, 0.2f);
    public float pulseSpeed = 1.5f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;

    private Renderer[] renderers;
    private MaterialPropertyBlock propBlock;
    private Material[] cachedMaterials;
    private bool isOutlineActive = false;
    private bool isHovering = false;

    // Offset aléatoire pour que tous les objets ne pulsent pas en sync
    private float timeOffset;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();
        timeOffset = Random.Range(0f, Mathf.PI * 2f);

        // Cache les matériaux UNE seule fois + active _EMISSION une seule fois
        var matList = new List<Material>();
        foreach (var r in renderers)
            foreach (var mat in r.sharedMaterials) 
                matList.Add(mat);
        cachedMaterials = matList.ToArray();

        foreach (var mat in cachedMaterials)
            mat.EnableKeyword("_EMISSION");
    }

    void OnEnable()
    {
        OutlinePulseManager.Register(this);
    }

    void OnDisable()
    {
        OutlinePulseManager.Unregister(this);
        SetEmissionDirect(Color.black);
    }

    public void SetOutline(bool active)
    {
        isOutlineActive = active;
        if (!active) SetEmissionDirect(Color.black);
    }

    // Appelé chaque frame par OutlinePulseManager — pas de coroutine individuelle
    public void Tick(float time)
    {
        if (!isOutlineActive) return;

        if (isHovering)
        {
            SetEmissionDirect(outlineColor * maxAlpha * 1.5f);
            return;
        }

        float t = Mathf.Sin((time + timeOffset) * pulseSpeed) * 0.5f + 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        SetEmissionDirect(outlineColor * alpha);
    }

    public void OnHoverEnter()
    {
        isHovering = true;
    }

    public void OnHoverExit()
    {
        isHovering = false;
    }

    // Interne — n'est appelé que depuis Tick() ou SetOutline()
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