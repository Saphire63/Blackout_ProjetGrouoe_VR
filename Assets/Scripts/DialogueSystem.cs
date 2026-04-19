using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dialogueText;
    public CanvasGroup canvasGroup;

    [Header("Paramètres")]
    public float distanceFromCamera = 2f;    // distance devant les yeux
    public float verticalOffset = -0.5f;     // position en bas du champ de vision
    public float fadeSpeed = 2f;
    public float typewriterSpeed = 0.04f;    // secondes entre chaque caractère

    private Camera vrCamera;
    private Coroutine currentDialogue;
    private bool isShowing = false;

    void Start()
    {
        vrCamera = Camera.main;
        canvasGroup.alpha = 0f;
        dialogueText.text = "";

        // Le canvas doit être en World Space
        GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        transform.localScale = Vector3.one * 0.002f; // échelle adaptée au World Space
    }

    void LateUpdate()
    {
        if (!isShowing) return;

        // Suivre la caméra VR
        Transform cam = vrCamera.transform;
        transform.position = cam.position
            + cam.forward * distanceFromCamera
            + cam.up * verticalOffset;
        transform.rotation = cam.rotation;
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

        // Effet typewriter
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        // Attendre la durée
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
