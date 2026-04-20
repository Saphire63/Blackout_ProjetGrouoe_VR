using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

/// <summary>
/// Place ce script sur un GameObject vide au pied/haut de chaque escalier.
/// Quand le joueur s'approche, un bouton flottant apparaît pour se téléporter.
///
/// SETUP :
/// - Crée un GameObject vide "StairsTeleporter_Up" au pied des escaliers
/// - Crée un GameObject vide "StairsTeleporter_Down" en haut des escaliers
/// - Assigne teleportDestination → le point d'arrivée (GameObject vide à l'étage)
/// - Assigne triggerRadius → rayon de détection (ex: 2 mètres)
/// </summary>
public class StairsTeleporter : MonoBehaviour
{
    [Header("Destination")]
    [Tooltip("Le point où le joueur sera téléporté (GameObject vide à placer à l'étage)")]
    public Transform teleportDestination;
    [Tooltip("Texte affiché sur le bouton (ex: 'Monter au 1er étage' ou 'Descendre à la cave')")]
    public string buttonLabel = "Monter au 1er étage";

    [Header("Détection")]
    [Tooltip("Distance à laquelle le bouton apparaît")]
    public float triggerRadius = 2f;

    [Header("UI du bouton")]
    public Canvas buttonCanvas;             // Canvas World Space avec le bouton
    public TextMeshProUGUI buttonText;      // TextMeshPro du label
    public GameObject buttonObject;         // Le visuel du bouton

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip teleportSound;

    [Header("Références VR")]
    public Transform xrOrigin;             // Le XR Origin (le joueur)

    // ─── Privé ──────────────────────────────────────────────
    private bool isVisible = false;
    private bool isTeleporting = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable buttonInteractable;
    private Transform playerCamera;

    void Awake()
    {
        playerCamera = Camera.main?.transform;

        // Auto-trouver le XR Origin si pas assigné
        if (xrOrigin == null)
        {
            var origin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (origin != null) xrOrigin = origin.transform;
        }

        // Setup du bouton
        if (buttonCanvas != null) buttonCanvas.gameObject.SetActive(false);
        if (buttonText != null) buttonText.text = buttonLabel;

        // Interactable sur le bouton
        if (buttonObject != null)
        {
            buttonInteractable = buttonObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            if (buttonInteractable == null)
                buttonInteractable = buttonObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            buttonInteractable.selectEntered.AddListener(OnButtonPressed);
        }
    }

    void Update()
    {
        if (playerCamera == null || isTeleporting) return;

        float distance = Vector3.Distance(transform.position, playerCamera.position);

        if (distance < triggerRadius && !isVisible)
            ShowButton();
        else if (distance >= triggerRadius && isVisible)
            HideButton();

        // Le bouton suit et fait face au joueur
        if (isVisible && buttonCanvas != null)
        {
            buttonCanvas.transform.position = transform.position + Vector3.up * 0.5f;
            buttonCanvas.transform.LookAt(playerCamera);
            buttonCanvas.transform.Rotate(0, 180f, 0);
        }
    }

    void ShowButton()
    {
        isVisible = true;
        if (buttonCanvas != null)
        {
            buttonCanvas.gameObject.SetActive(true);
            StartCoroutine(FadeButton(0f, 1f));
        }
    }

    void HideButton()
    {
        isVisible = false;
        StartCoroutine(FadeAndHide());
    }

    void OnButtonPressed(SelectEnterEventArgs args)
    {
        if (isTeleporting || teleportDestination == null) return;
        StartCoroutine(TeleportRoutine());
    }

    IEnumerator TeleportRoutine()
    {
        isTeleporting = true;
        HideButton();

        // Fade au noir
        yield return StartCoroutine(FadeScreen(0f, 1f, 0.3f));

        // Téléporter
        if (xrOrigin != null)
            xrOrigin.position = teleportDestination.position;

        if (audioSource && teleportSound)
            audioSource.PlayOneShot(teleportSound);

        yield return new WaitForSeconds(0.1f);

        // Fade retour
        yield return StartCoroutine(FadeScreen(1f, 0f, 0.3f));

        isTeleporting = false;
    }

    IEnumerator FadeScreen(float from, float to, float duration)
    {
        // Simple fade via une image noire sur la caméra
        // Si tu as un ScreenFader dans ta scène, utilise-le ici
        // Sinon on fait juste une pause
        yield return new WaitForSeconds(duration);
    }

    IEnumerator FadeButton(float from, float to)
    {
        if (buttonCanvas == null) yield break;
        var group = buttonCanvas.GetComponent<CanvasGroup>();
        if (group == null) group = buttonCanvas.gameObject.AddComponent<CanvasGroup>();

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            group.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        group.alpha = to;
    }

    IEnumerator FadeAndHide()
    {
        yield return StartCoroutine(FadeButton(1f, 0f));
        if (buttonCanvas != null) buttonCanvas.gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        // Visualiser le rayon de détection dans la Scene View
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
