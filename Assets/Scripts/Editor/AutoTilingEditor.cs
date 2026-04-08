using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutoTiling))]
[CanEditMultipleObjects] // Permet de modifier plusieurs objets à la fois
public class AutoTilingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // --- Section Globale ---
        EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        float newBrick = EditorGUILayout.FloatField("Brick Size (exterior)", AutoTilingGlobal.globalBrickSize);
        float newTile = EditorGUILayout.FloatField("Tile Size (carrelage)", AutoTilingGlobal.globalTileSize);

        if (EditorGUI.EndChangeCheck())
        {
            AutoTilingGlobal.globalBrickSize = newBrick;
            AutoTilingGlobal.globalTileSize = newTile;
        }

        // Bouton pour appliquer à TOUT le projet manuellement
        if (GUILayout.Button("Force Update All Objects in Scene"))
        {
            AutoTilingGlobal.ManualSyncAll();
        }

        EditorGUILayout.Space();

        // --- Section Objet Sélectionné ---
        EditorGUILayout.LabelField("Per Object Settings", EditorStyles.boldLabel);

        // On dessine l'inspecteur par défaut (tileType, brickSize, etc.)
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            // Si on change un truc sur l'objet, on applique le tiling direct
            foreach (var targetObject in targets)
            {
                ((AutoTiling)targetObject).ApplyTiling();
            }
        }
    }
}

// Classe statique pour stocker les réglages sans faire ramer l'Update
public static class AutoTilingGlobal
{
    public static float globalBrickSize = 2f;
    public static float globalTileSize = 0.5f;

    public static void ManualSyncAll()
    {
        // On ne cherche les objets que si l'utilisateur clique sur le bouton
        AutoTiling[] all = Object.FindObjectsByType<AutoTiling>(FindObjectsSortMode.None);
        foreach (AutoTiling at in all)
        {
            Undo.RecordObject(at, "Global AutoTiling Update"); // Permet de faire Ctrl+Z
            at.brickSize = at.tileType == TileType.Brick
                ? globalBrickSize
                : globalTileSize;
            at.ApplyTiling();
        }
        Debug.Log($"Mise à jour de {all.Length} objets AutoTiling terminée !");
    }
}