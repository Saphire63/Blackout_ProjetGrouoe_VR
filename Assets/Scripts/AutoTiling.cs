using UnityEngine;

[ExecuteInEditMode]
public class AutoTiling : MonoBehaviour
{
    public float tilingScale = 1f;

    void Update()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            if (r != null)
            {
                r.material.mainTextureScale = new Vector2(
                    transform.localScale.x * tilingScale,
                    transform.localScale.y * tilingScale
                );
            }
        }
    }
}