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
    public GameObject[] interactableObjects; // tous les objets interactables de la scène

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
                dialogueSystem.ShowDialogue("Enfin chez moi... J'ai qu'une envie, m'installer et regarder un film.", 4f, () => {
                    // Le joueur entre dans la maison et allume la lumière
                    // Le trigger PowerOutage sera déclenché par le switch de lumière
                });
                break;

            case GameState.PowerOutage:
                dialogueSystem.ShowDialogue("Quoi ?! Encore une coupure...", 3f, () => {
                    SetState(GameState.CandleLit); // après avoir trouvé la bougie
                });
                break;

            case GameState.CandleLit:
                dialogueSystem.ShowDialogue("Il faut que j'aille rétablir le courant à la cave.", 3f, () =>
                {
                    EnableInteractables(true);
                });
                break;

            case GameState.SearchingKeyRDC:
                dialogueSystem.ShowDialogue("La porte est fermée à clé... Où est-ce que je l'ai mise ?", 3f, null);
                // Après avoir cherché la moitié du RDC, un trigger géographique lance la ligne suivante
                break;

            case GameState.SearchingKeyUpstairs:
                dialogueSystem.ShowDialogue("Attends... je crois que je l'ai laissée au premier étage.", 3f, null);
                break;

            case GameState.HasKey:
                dialogueSystem.ShowDialogue("La voilà ! Maintenant je peux ouvrir la porte de la cave.", 3f, null);
                break;

            case GameState.BasementOpen:
                dialogueSystem.ShowDialogue("Le tableau électrique doit être par ici...", 3f, null);
                break;

            case GameState.PowerRestored:
                powerOutage.RestorePower();
                dialogueSystem.ShowDialogue("Voilà ! La lumière est de retour.", 3f, () => {
                    SetState(GameState.Epilogue);
                });
                break;

            case GameState.Epilogue:
                dialogueSystem.ShowDialogue("Bien. Maintenant, ce film que j'attends depuis ce matin...", 4f, null);
                break;
        }
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
