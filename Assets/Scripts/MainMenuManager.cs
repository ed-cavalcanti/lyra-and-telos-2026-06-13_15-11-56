using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highScoreText;
    
    // Trava para evitar que o jogador clique várias vezes no botão "Jogar"
    private bool isStartingGame = false;

    void Start()
    {
        // Exibe o recorde salvo ao abrir o menu
        int recorde = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = "Recorde: " + recorde.ToString();
    }

    public void Jogar()
    {
        // Se já estiver carregando o jogo, ignora os próximos cliques
        if (isStartingGame) return;
        
        isStartingGame = true;

        // Tenta encontrar o TransitionManager na cena
        TransitionManager tm = TransitionManager.Instance;
        if (tm == null)
        {
            tm = FindAnyObjectByType<TransitionManager>();
        }

        // Se achou o gerenciador, faz o fade e carrega a cena no escuro
        if (tm != null)
        {
            tm.DoTransition(() => 
            {
                SceneManager.LoadScene("StartCutscene");
            });
        }
        else
        {
            // Segurança: Se esquecer de colocar o prefab do fade na cena do Menu, 
            // ele avisa no console mas carrega a cena direto para não travar o jogo.
            Debug.LogWarning("[MainMenuManager] TransitionManager não encontrado nesta cena! Carregando direto.");
            SceneManager.LoadScene("StartCutscene");
        }
    }

    public void Sair()
    {
        Debug.Log("Saindo do Jogo...");
        Application.Quit();
    }
}