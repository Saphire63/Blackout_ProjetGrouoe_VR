using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ElectricalPanel : MonoBehaviour
{
    [Header("Boutons")]
    public ElectricalButton[] buttons;  // tous les boutons du tableau
    public int correctButtonIndex = 2;  // index du bon bouton (à définir dans l'Inspector)

    [Header("Audio")]
    public AudioSource panelAudio;
    public AudioClip wrongButtonSound;
    public AudioClip correctButtonSound;

    private bool isSolved = false;

    public void OnButtonPressed(int buttonIndex)
    {
        if (isSolved) return;

        if (buttonIndex == correctButtonIndex)
        {
            CorrectButton();
        }
        else
        {
            WrongButton(buttonIndex);
        }
    }

    void WrongButton(int index)
    {
        if (panelAudio && wrongButtonSound)
            panelAudio.PlayOneShot(wrongButtonSound);

        GameManager.Instance.dialogueSystem.ShowDialogue(
            "Non, c'est pas celui-là...", 2f, null);

        // Optionnel : feedback visuel sur le mauvais bouton
        if (buttons[index] != null)
            buttons[index].FlashRed();
    }

    void CorrectButton()
    {
        isSolved = true;

        if (panelAudio && correctButtonSound)
            panelAudio.PlayOneShot(correctButtonSound);

        GameManager.Instance.dialogueSystem.ShowDialogue("Voilà !", 2f, () => {
            GameManager.Instance.SetState(GameState.PowerRestored);
        });
    }
}

// ─────────────────────────────────────────────
// Classe pour chaque bouton individuel
// ─────────────────────────────────────────────
public class ElectricalButton : MonoBehaviour
{
    public int buttonIndex;
    public ElectricalPanel panel;

    [Header("Feedback visuel")]
    public Renderer buttonRenderer;
    public Color normalColor = Color.gray;
    public Color pressedColor = Color.red;
    public Color correctColor = Color.green;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private MaterialPropertyBlock propBlock;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnPress);
        propBlock = new MaterialPropertyBlock();
    }

    void OnPress(SelectEnterEventArgs args)
    {
        panel.OnButtonPressed(buttonIndex);
    }

    public void FlashRed()
    {
        StartCoroutine(FlashRoutine(pressedColor, 0.5f));
    }

    public void SetGreen()
    {
        SetColor(correctColor);
    }

    IEnumerator FlashRoutine(Color flashColor, float duration)
    {
        SetColor(flashColor);
        yield return new WaitForSeconds(duration);
        SetColor(normalColor);
    }

    void SetColor(Color color)
    {
        if (buttonRenderer == null) return;
        buttonRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_BaseColor", color);
        buttonRenderer.SetPropertyBlock(propBlock);
    }
}
