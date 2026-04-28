using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Tiroir — interaction via Ray Interactor.
/// Quand on l'ouvre, l'item caché (ex: la clé) apparaît avec sa surbrillance.
/// Après X ouvertures sans trouver la clé, indique au joueur de chercher au 1er étage.
///
/// SETUP dans Unity :
///  DrawerBody (ce script + XRSimpleInteractable sur la poignée enfant)
///  ├── DrawerMesh
///  ├── Handle (XRSimpleInteractable)
///  └── HiddenItem (la clé, désactivée au départ)
/// </summary>
public class DrawerInteractable : MonoBehaviour
{
    [Header("Transform du tiroir")]
    [Tooltip("Le Transform qui glisse. Si vide, utilise ce GameObject.")]
    public Transform drawerTransform;

    [Header("Mouvement")]
    [Tooltip("Direction locale de glissement (Vector3.back = vers l'avant en général).")]
    public Vector3 slideDirection = Vector3.back;
    public float slideDistance = 0.3f;
    [Range(1f, 8f)]
    public float slideSpeed = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    [Header("Item révélé à l'ouverture")]
    [Tooltip("Objet caché dans le tiroir (ex: KeyItem). Sera activé à l'ouverture.")]
    public GameObject hiddenItem;

    [Header("Indice après X ouvertures")]
    [Tooltip("Nombre d'ouvertures de tiroirs (tous confondus) avant d'afficher l'indice '1er étage'.")]
    public int interactionsBeforeHint = 5;

    // Compteur et flag partagés entre toutes les instances
    private static int totalOpenings = 0;
    private static bool hintShown = false;

    // ─── Privé ──────────────────────────────────────────────
    private bool isOpen = false;
    private bool isAnimating = false;
    private Vector3 closedLocalPos;
    private Vector3 openLocalPos;

    void Awake()
    {
        if (drawerTransform == null) drawerTransform = transform;

        closedLocalPos = drawerTransform.localPosition;
        Vector3 scaledDirection = slideDirection.normalized;
        if (drawerTransform.parent != null)
        {
            Vector3 parentScale = drawerTransform.parent.lossyScale;
            scaledDirection = new Vector3(
                scaledDirection.x / Mathf.Max(parentScale.x, 0.0001f),
                scaledDirection.y / Mathf.Max(parentScale.y, 0.0001f),
                scaledDirection.z / Mathf.Max(parentScale.z, 0.0001f)
            );
        }
        openLocalPos = closedLocalPos + scaledDirection * slideDistance;

        var handle = GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (handle != null)
        {
            handle.selectEntered.AddListener(OnRaySelect);
            handle.hoverEntered.AddListener(OnHoverEnter);
            handle.hoverExited.AddListener(OnHoverExit);
        }

        if (hiddenItem != null) hiddenItem.SetActive(false);

        // Reset au démarrage de la scène
        totalOpenings = 0;
        hintShown = false;
    }

    // ─── Events Ray Interactor ───────────────────────────────

    void OnRaySelect(SelectEnterEventArgs args)
    {
        if (isAnimating) return;

        if (isOpen)
        {
            CloseDrawer();
        }
        else
        {
            OpenDrawer();

            // Comptage uniquement si on cherche encore la clé au RDC
            if (!hintShown && hiddenItem == null &&
                (GameManager.Instance.currentState == GameState.CandleLit ||
                 GameManager.Instance.currentState == GameState.SearchingKeyRDC))
            {
                totalOpenings++;

                if (totalOpenings >= interactionsBeforeHint)
                {
                    hintShown = true;
                    GameManager.Instance.SetState(GameState.SearchingKeyUpstairs);
                }
            }
        }
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        var outline = GetComponentInChildren<OutlineController>();
        outline?.OnHoverEnter();
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        var outline = GetComponentInChildren<OutlineController>();
        outline?.OnHoverExit();
    }

    // ─── Ouverture / Fermeture ───────────────────────────────

    public void OpenDrawer()
    {
        if (isOpen || isAnimating) return;
        isOpen = true;
        PlaySound(openSound);
        StartCoroutine(SlideRoutine(openLocalPos, RevealItem));
    }

    public void CloseDrawer()
    {
        if (!isOpen || isAnimating) return;
        isOpen = false;
        PlaySound(closeSound);
        StartCoroutine(SlideRoutine(closedLocalPos, null));
    }

    void RevealItem()
    {
        if (hiddenItem == null) return;

        hiddenItem.SetActive(true);

        var drawerInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (drawerInteractable != null) drawerInteractable.enabled = false;

        var key = hiddenItem.GetComponent<KeyItem>();
        if (key != null)
            GameManager.Instance.dialogueSystem.ShowDialogue("La voilà !", 2f, null);
    }

    IEnumerator SlideRoutine(Vector3 targetPos, System.Action onComplete)
    {
        isAnimating = true;
        Vector3 startPos = drawerTransform.localPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * slideSpeed;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            drawerTransform.localPosition = Vector3.Lerp(startPos, targetPos, ease);
            yield return null;
        }

        drawerTransform.localPosition = targetPos;
        isAnimating = false;
        onComplete?.Invoke();
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource && clip) audioSource.PlayOneShot(clip);
    }

    public bool IsOpen() => isOpen;
}