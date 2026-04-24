using System.Collections;
using UnityEngine;

public enum GameState
{
    Intro,
    PowerOn,
    PowerOutage,
    CandleLit,
    SearchingKeyRDC,
    SearchingKeyUpstairs,
    HasKey,
    BasementOpen,
    PowerRestored,
    Epilogue
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Références")]
    public DialogueSystem dialogueSystem;
    public PowerOutage powerOutage;
    public GameObject[] interactableObjects;

    public GameState currentState { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SetState(GameState.Intro);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        HandleState(newState);
    }

    void HandleState(GameState state)
    {
        switch (state)
        {
            case GameState.Intro:
                dialogueSystem.ShowDialogue(
                    "Enfin chez moi... J'ai qu'une envie, m'installer et regarder un film.", 4f, null);
                break;

            case GameState.PowerOn:
                break;

            case GameState.PowerOutage:
                EnableInteractables(true);  // ← bougie en surbrillance dès le noir
                dialogueSystem.ShowDialogue("Quoi ?! Encore une coupure...", 3f, () => {
                    StartCoroutine(DelayedDialogue(
                        "Il me semblait avoir mis une bougie quelque part...", 2f, null));
                });
                break;

            case GameState.CandleLit:
                dialogueSystem.ShowDialogue(
                    "Il faut que j'aille rétablir le courant à la cave.", 3f, () => {
                        EnableInteractables(true);  
                        StartCoroutine(DelayedDialogue(
                            "L'accès à la cave se fait par la cuisine.", 3f, null));
                    });
                break;

            case GameState.SearchingKeyRDC:
                dialogueSystem.ShowDialogue(
                    "C'est fermé à clé... il me faut la clé du sous-sol.", 3f, null);
                break;

            case GameState.SearchingKeyUpstairs:
                dialogueSystem.ShowDialogue(
                    "Attends... je crois que je l'ai laissée au premier étage.", 3f, null);
                break;

            case GameState.HasKey:
                dialogueSystem.ShowDialogue(
                    "Maintenant je peux ouvrir la porte de la cave.", 3f, null);
                break;

            case GameState.BasementOpen:
                dialogueSystem.ShowDialogue(
                    "Le tableau électrique doit être par ici...", 3f, null);
                break;

            case GameState.PowerRestored:
                powerOutage.RestorePower();
                dialogueSystem.ShowDialogue("Voilà ! La lumière est de retour.", 3f, () => {
                    SetState(GameState.Epilogue);
                });
                break;

            case GameState.Epilogue:
                dialogueSystem.ShowDialogue("Bien. Maintenant, mon film...", 4f, null);
                break;
        }
    }

    public IEnumerator DelayedDialogue(string text, float delay, System.Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        dialogueSystem.ShowDialogue(text, 3f, onComplete);
    }

    public void EnableInteractables(bool enable)
    {
        foreach (var obj in interactableObjects)
        {
            if (obj != null)
            {
                var outline = obj.GetComponent<OutlineController>();
                if (outline != null) outline.SetOutline(enable);
            }
        }
    }
}