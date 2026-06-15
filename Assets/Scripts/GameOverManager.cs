using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private GameObject gameOverPanel; // Arraste o GameOver_Panel aqui

    public void ExibirGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // === NOVO: Método que o seu botão de "Continuar" vai chamar ===
    public void ContinuarDoCheckpoint()
    {
        // 1. O tempo volta a correr
        Time.timeScale = 1f;

        // 2. Esconde a tela de morte
        gameOverPanel.SetActive(false);

        // 3. Encontra o jogador na cena e manda ele reviver no checkpoint
        PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
        if (player != null)
        {
            player.RespawnAtCheckpoint();
        }
    }

    // Mantive a função original caso você queira um botão de "Reiniciar Fase Inteira"
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