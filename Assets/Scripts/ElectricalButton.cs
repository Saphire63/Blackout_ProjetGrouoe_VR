using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ElectricalButton : MonoBehaviour
{
    [Header("Index de ce bouton")]
    public int buttonIndex;

    [Header("Référence au tableau")]
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
        if (interactable == null)
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        interactable.selectEntered.AddListener(OnPress);
        propBlock = new MaterialPropertyBlock();
    }

    void OnPress(SelectEnterEventArgs args)
    {
        if (panel != null)
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
