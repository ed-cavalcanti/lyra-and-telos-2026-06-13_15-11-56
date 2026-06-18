using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private GameObject gameOverPanel;

    public void ExibirGameOver()
    {
        // BLINDAGEM: Verifica se o painel de fato existe nesta instância do script
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f; // Pausa o jogo
        }
        else
        {
            Debug.LogError("[GameOverManager] O painel de Game Over não foi atribuído! Verifique se você não tem dois GameOverManagers na cena ou se o Evento do HealthUI está puxando o objeto errado.");
        }
    }

    public void ContinuarDoCheckpoint()
    {
        // ESCONDE O MENU IMEDIATAMENTE
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // BLINDAGEM: Verifica se o TransitionManager existe!
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.DoTransition(() =>
            {
                ExecutarRespawn();
            });
        }
        else
        {
            Debug.LogWarning("[GameOverManager] TransitionManager não encontrado! Dando respawn direto.");
            ExecutarRespawn();
        }
    }

    // Função auxiliar para não repetir o código de respawn
    private void ExecutarRespawn()
    {
        PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
        if (player != null)
        {
            player.RespawnAtCheckpoint();
            Time.timeScale = 1f;
        }
    }

    public void ReiniciarCena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // === NOVA FUNÇÃO EXCLUSIVA PARA A CENA DO BOSS ===
    public void ReiniciarCenaDoBoss()
    {
        // Esconde o painel imediatamente para a transição ficar bonita
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Despausa o jogo
        Time.timeScale = 1f;

        // O SEGREDO: Apaga a "memória" de morte. O valor -1 força o PlayerHealth a usar o MaxHealth!
        PlayerHealth.globalSavedHealth = -1;

        // Transição suave blindada
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.DoTransition(() =>
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        }
        else
        {
            Debug.LogWarning("[GameOverManager] TransitionManager ausente. Recarregando cena do boss direto.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;

        // BLINDAGEM DO MENU
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TransitionToScene("MainMenu");
        }
        else
        {
            Debug.LogWarning("[GameOverManager] TransitionManager não encontrado! Voltando ao menu direto.");
            SceneManager.LoadScene("MainMenu");
        }
    }
}