using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Configurações do Portal")]
    [Tooltip("Nome EXATO da cena que será carregada (ex: Level_2)")]
    [SerializeField] private string sceneToLoad;

    private bool isPlayerNear = false;
    private bool isTransitioning = false; // Trava de segurança

    private void Update()
    {
        // Se o jogador estiver perto e a transição ainda não começou
        if (isPlayerNear && !isTransitioning)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                AtivarPortal();
            }
        }
    }

    private void AtivarPortal()
    {
        isTransitioning = true; // Impede que o jogador aperte E várias vezes
        Debug.Log("Carregando a fase: " + sceneToLoad);

        // Opcional: Adicionar um som de portal aqui
        // AudioManager.Instance.PlaySFX("PortalSound");

        // BLINDAGEM ANTI-BUG: Verifica se o TransitionManager existe!
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.DoTransition(() =>
            {
                // === TUDO AQUI DENTRO ACONTECE NA TELA PRETA ===
                SceneManager.LoadScene(sceneToLoad);
            });
        }
        else
        {
            // Se o TransitionManager não estiver na cena, carrega a fase direto (seco)
            Debug.LogWarning("[Portal] TransitionManager não encontrado! Trocando de cena diretamente.");
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}