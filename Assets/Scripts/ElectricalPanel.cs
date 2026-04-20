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

