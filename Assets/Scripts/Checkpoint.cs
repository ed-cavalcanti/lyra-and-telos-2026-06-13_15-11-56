using UnityEngine;
using UnityEngine.InputSystem;

public class Checkpoint : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Animator animator; // Arraste o Animator da Estátua aqui
    [SerializeField] private Transform spawnPoint;

    [Header("Configurações")]
    [SerializeField] private string animationParam = "isActivated";

    [SerializeField] private GameObject interactPrompt;

    private bool isPlayerNear = false;
    private bool isActivated = false;
    private PlayerHealth playerHealth;

    private void Start()
    {
        // Garante que o aviso comece invisível quando o jogo iniciar
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        // O jogador pode apertar 'E' para salvar a qualquer momento se estiver perto,
        // mas a animação de ativação só deve acontecer na primeira vez.

        if (isPlayerNear)
        {
            // O aviso só aparece se o jogador estiver perto E o diálogo NÃO estiver rolando
            interactPrompt.SetActive(true);
        }
        else
        {
            interactPrompt.SetActive(false);
        }

        // Se o jogador estiver perto e apertar a tecla "E", inicia o diálogo
        if (isPlayerNear && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SaveGameProgress();
        }
    }

    private void SaveGameProgress()
    {
        // 1. Se for a primeira vez ativando, disparar a animação
        if (!isActivated)
        {
            isActivated = true;
            if (animator != null)
            {
                animator.SetBool(animationParam, true);
            }
            Debug.Log("Estátua ativada pela primeira vez!");
        }

        // 2. Salva a posição no script de vida do jogador
        Vector2 savePosition = spawnPoint != null ? spawnPoint.position : transform.position;
        if (playerHealth != null)
        {
            playerHealth.SetCheckpoint(savePosition);

            // Dica: Você pode adicionar um som ou um efeito de partículas de "Save" aqui
            Debug.Log("Progresso salvo no checkpoint!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerHealth = collision.GetComponent<PlayerHealth>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerHealth = null;
        }
    }
}