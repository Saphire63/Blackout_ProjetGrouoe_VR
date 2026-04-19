using UnityEngine;

/// <summary>
/// Placer ce trigger au milieu du RDC.
/// Quand le joueur le traverse en cherchant la clé, il se souvient que c'est au 1er.
/// </summary>
public class MidFloorTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        // Seulement si on est dans l'état de recherche au RDC
        if (GameManager.Instance.currentState == GameState.SearchingKeyRDC)
        {
            hasTriggered = true;
            GameManager.Instance.SetState(GameState.SearchingKeyUpstairs);
        }
    }
}
