using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dialogueText;
    public CanvasGroup canvasGroup;

    [Header("Paramètres VR")]
    public float distanceFromCamera = 0.5f;
    public float verticalOffset = -0.4f;
    public float fadeSpeed = 2f;
    public float typewriterSpeed = 0.04f;   // secondes entre chaque caractère

    private Camera vrCamera;
    private Coroutine currentDialogue;
    private bool isShowing = false;

    void Start()
    {
        vrCamera = Camera.main;
        canvasGroup.alpha = 0f;
        dialogueText.text = "";

        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.localScale = Vector3.one * 0.002f;
        }
    }

    void LateUpdate()
    {
        if (!isShowing || vrCamera == null) return;

        transform.position = vrCamera.transform.position
            + vrCamera.transform.forward * distanceFromCamera
            + vrCamera.transform.up * verticalOffset;
        transform.rotation = vrCamera.transform.rotation;
    }

    public void ShowDialogue(string text, float duration, System.Action onComplete)
    {
        if (currentDialogue != null) StopCoroutine(currentDialogue);
        currentDialogue = StartCoroutine(DialogueRoutine(text, duration, onComplete));
    }

    IEnumerator DialogueRoutine(string text, float duration, System.Action onComplete)
    {
        isShowing = true;

        // Fade in
        yield return StartCoroutine(Fade(0f, 1f));

        // Typewriter
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        // Attendre
        yield return new WaitForSeconds(duration);

        // Fade out
        yield return StartCoroutine(Fade(1f, 0f));

        dialogueText.text = "";
        isShowing = false;

        onComplete?.Invoke();
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        canvasGroup.alpha = to;
    }

    public void ClearDialogue()
    {
        if (currentDialogue != null) StopCoroutine(currentDialogue);
        canvasGroup.alpha = 0f;
        dialogueText.text = "";
        isShowing = false;
    }
}