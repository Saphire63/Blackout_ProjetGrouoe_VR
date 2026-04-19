using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class TVCredits : MonoBehaviour
{
    [Header("Télévision")]
    public Renderer tvScreenRenderer;       // le renderer de l'écran de la TV
    public Material screenOffMaterial;
    public Material screenOnMaterial;
    public Light tvGlow;                    // lumière ambiante de la TV (bleutée)
    public AudioSource tvAudio;
    public AudioClip tvOnSound;

    [Header("Crédits")]
    public Canvas creditsCanvas;            // Canvas World Space sur l'écran de la TV
    public TextMeshProUGUI creditsText;
    public float creditsScrollSpeed = 30f;

    [Header("Texte des crédits")]
    [TextArea(10, 30)]
    public string creditsContent = @"
UNE SOIRÉE TRANQUILLE

Un jeu de...
[Ton Nom / Studio]

Réalisation
[Ton Nom]

Programmation
[Ton Nom]

Modélisation 3D
[Nom]

Sound Design
[Nom]

Merci d'avoir joué.

Bonne soirée.
Et bon film.
";

    private bool isOn = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private RectTransform creditsRect;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnTVInteract);

        if (creditsCanvas) creditsCanvas.gameObject.SetActive(false);
        if (tvGlow) tvGlow.enabled = false;
    }

    void OnTVInteract(SelectEnterEventArgs args)
    {
        // Seulement si le courant est rétabli
        if (GameManager.Instance.currentState != GameState.Epilogue) return;
        if (isOn) return;

        TurnOnTV();
    }

    void TurnOnTV()
    {
        isOn = true;

        // Changer le matériau de l'écran
        if (tvScreenRenderer && screenOnMaterial)
            tvScreenRenderer.material = screenOnMaterial;

        // Allumer le glow bleuté
        if (tvGlow) tvGlow.enabled = true;

        // Son
        if (tvAudio && tvOnSound)
            tvAudio.PlayOneShot(tvOnSound);

        // Lancer les crédits
        StartCoroutine(ShowCredits());
    }

    IEnumerator ShowCredits()
    {
        yield return new WaitForSeconds(1f);

        if (creditsCanvas == null) yield break;

        creditsCanvas.gameObject.SetActive(true);
        creditsText.text = creditsContent;
        creditsRect = creditsText.GetComponent<RectTransform>();

        // Placer le texte en bas pour qu'il monte
        Vector2 startPos = new Vector2(0, -600f);
        Vector2 endPos = new Vector2(0, 600f + creditsText.preferredHeight);
        creditsRect.anchoredPosition = startPos;

        float duration = (endPos.y - startPos.y) / creditsScrollSpeed;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            creditsRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t / duration);
            yield return null;
        }

        // Fin du jeu
        yield return new WaitForSeconds(3f);
        GameManager.Instance.dialogueSystem.ShowDialogue("Fin.", 5f, null);
    }
}
