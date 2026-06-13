using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highScoreText;

    void Start()
    {
        // Exibe o recorde salvo ao abrir o menu
        int recorde = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = "Recorde: " + recorde.ToString();
    }

    public void Jogar()
    {
        SceneManager.LoadScene("Palestone"); // Mude para o nome da sua cena
    }

    public void Sair()
    {
        Debug.Log("Saindo do Jogo...");
        Application.Quit();
    }
}
