using UnityEngine;
using UnityEngine.InputSystem;

public class NpcBehavior : MonoBehaviour
{
    [Header("Configurações do NPC")]
    public DialogueData dialogueData;

    [SerializeField] private GameObject interactPrompt;

    private DialogueSystem dialogueSystem;
    private bool isPlayerInRange = false;

    void Start()
    {
        dialogueSystem = FindAnyObjectByType<DialogueSystem>(FindObjectsInactive.Include);

        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueSystem não encontrado na cena! Certifique-se de que ele está presente.");
        }

        // Garante que o aviso comece invisível quando o jogo iniciar
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    void Update()
    {
        // Gerencia a visibilidade do aviso visual
        if (isPlayerInRange && interactPrompt != null && dialogueSystem != null)
        {
            // O aviso só aparece se o jogador estiver perto E o diálogo NÃO estiver rolando
            interactPrompt.SetActive(!dialogueSystem.IsDialogueActive);
        }

        // Se o jogador estiver perto e apertar a tecla "E", inicia o diálogo
        if (isPlayerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (dialogueSystem != null && !dialogueSystem.IsDialogueActive)
            {
                dialogueSystem.StartDialogue(dialogueData);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }
}
