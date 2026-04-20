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
    
    // On garde la liste mais on n'utilisera plus d'Outline dessus
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
                dialogueSystem.ShowDialogue("Enfin chez moi... J'ai qu'une envie, m'installer et regarder un film.", 4f, null);
                break;

            case GameState.PowerOn:
                // État temporaire quand on allume l'interrupteur
                break;

            case GameState.PowerOutage:
                // Le texte est maintenant géré directement par le PowerOutage ou ici
                break;

            case GameState.SearchingKeyRDC:
                dialogueSystem.ShowDialogue("C'est verrouillé... il me faut la clé de la cave. Elle doit être dans un tiroir.", 4f, null);
                break;

            case GameState.HasKey:
                dialogueSystem.ShowDialogue("La voilà ! Maintenant je peux ouvrir la porte de la cave.", 3f, null);
                break;

            case GameState.BasementOpen:
                dialogueSystem.ShowDialogue("Le tableau électrique doit être par ici...", 3f, null);
                break;

            case GameState.PowerRestored:
                dialogueSystem.ShowDialogue("Voilà ! La lumière est de retour.", 3f, () => {
                    SetState(GameState.Epilogue);
                });
                break;

            case GameState.Epilogue:
                dialogueSystem.ShowDialogue("Bien. Maintenant, mon film...", 4f, null);
                break;
        }
    }

    // FONCTION CORRIGÉE : On a enlevé l'OutlineController qui causait l'erreur
    public void EnableInteractables(bool enable)
    {
        // On laisse cette fonction vide pour l'instant pour éviter les erreurs
        // Tu pourras rajouter un autre système de surbrillance plus tard
    }
}