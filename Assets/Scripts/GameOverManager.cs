using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private GameObject gameOverPanel;

    public void ExibirGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // Pausa o jogo
    }

    public void ContinuarDoCheckpoint()
    {
        // 1. O tempo volta a correr imediatamente
        Time.timeScale = 1f;

        // 2. ESCONDE O MENU IMEDIATAMENTE (Assim o Fade acontece sobre o jogo!)
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 3. Inicia a transição de tela preta
        TransitionManager.Instance.DoTransition(() =>
        {
            // TUDO AQUI DENTRO SÓ ACONTECE NO ESCURO (O teleporte)
            PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
            if (player != null)
            {
                player.RespawnAtCheckpoint();
            }
        });
    }

    public void ReiniciarCena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }
}