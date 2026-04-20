using UnityEngine;

public enum TileType { Brick, Tile }

[ExecuteInEditMode]
public class AutoTiling : MonoBehaviour
{
    public TileType tileType = TileType.Brick;
    public float brickSize = 2f;

    private MaterialPropertyBlock _propBlock;
    private Renderer[] _cachedRenderers; // Cache pour éviter GetComponents
    private Vector3 _lastScale;

    // On ne fait plus rien dans Update !
    void Update()
    {
        // On ne met à jour que si le scale a changé
        if (transform.hasChanged)
        {
            ApplyTiling();
            transform.hasChanged = false;
        }
    }

    public void ApplyTiling()
    {
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        // On récupère les renderers seulement si nécessaire
        if (_cachedRenderers == null || _cachedRenderers.Length == 0)
            _cachedRenderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in _cachedRenderers)
        {
            if (r == null) continue;
            Vector3 size = r.bounds.size;

            bool isFloor = size.y < size.x && size.y < size.z;
            float width = size.x < 0.01f ? size.z : size.x;

            Vector2 newScale = isFloor
                ? new Vector2(size.x / brickSize, size.z / brickSize)
                : new Vector2(width / brickSize, size.y / brickSize);

            r.GetPropertyBlock(_propBlock);
            _propBlock.SetVector("_BaseMap_ST", new Vector4(newScale.x, newScale.y, 0f, 0f));
            r.SetPropertyBlock(_propBlock);
        }
    }
}