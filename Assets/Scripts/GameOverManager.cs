using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private GameObject gameOverPanel; // Arraste o GameOver_Panel aqui

    // Este método será chamado pelo UnityEvent OnDeath do jogador
    public void ExibirGameOver()
    {
        gameOverPanel.SetActive(true);
        
        // Congelar o tempo é opcional no Game Over, mas evita que 
        // inimigos continuem se movendo ou atacando a tela de morte.
        Time.timeScale = 0f; 
    }

    public void JogarNovamente()
    {
        // Regra de Ouro: Sempre devolva o tempo ao normal antes de recarregar a cena!
        Time.timeScale = 1f; 
        
        // Recarrega a cena atual, seja ela qual for
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        // Substitua pelo nome exato da sua Scene de Menu (ex: "Menu_Scene")
        SceneManager.LoadScene("MenuPrincipal"); 
    }
}