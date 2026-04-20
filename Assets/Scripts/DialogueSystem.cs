using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dialogueText;
    public CanvasGroup canvasGroup;

    [Header("Paramètres VR")]
    public float distanceFromCamera = 2.2f;
    public float verticalOffset = -0.4f;
    public float fadeSpeed = 2f;

    private Camera vrCamera;
    private bool isShowing = false;

    void Start()
    {
        vrCamera = Camera.main;
        canvasGroup.alpha = 0f;
        
        // On s'assure que le canvas est bien configuré pour la VR
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null) canvas.renderMode = RenderMode.WorldSpace;
    }

    void LateUpdate()
    {
        if (!isShowing || vrCamera == null) return;

        // Le texte reste collé à ton regard (comme avant)
        Transform cam = vrCamera.transform;
        transform.position = cam.position + cam.forward * distanceFromCamera + cam.up * verticalOffset;
        transform.rotation = cam.rotation;
    }

    public void ShowDialogue(string text, float duration, System.Action onComplete)
    {
        StopAllCoroutines();
        StartCoroutine(DialogueRoutine(text, duration, onComplete));
    }

    IEnumerator DialogueRoutine(string text, float duration, System.Action onComplete)
    {
        isShowing = true;
        dialogueText.text = text;

        // Fade In
        while (canvasGroup.alpha < 1f) { canvasGroup.alpha += Time.deltaTime * fadeSpeed; yield return null; }

        yield return new WaitForSeconds(duration);

        // Fade Out
        while (canvasGroup.alpha > 0f) { canvasGroup.alpha -= Time.deltaTime * fadeSpeed; yield return null; }

        isShowing = false;
        onComplete?.Invoke();
    }
}