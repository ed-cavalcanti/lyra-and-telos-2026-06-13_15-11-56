using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;
    private DialogueSystem dialogueSystem;

    void Start()
    {
        // Busca o sistema de diálogo, mesmo que comece desativado na cena
        dialogueSystem = FindAnyObjectByType<DialogueSystem>(FindObjectsInactive.Include);
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Checa se o jogador apertou ESC ou P nesta frame
        if (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame)
        {
            // A MÁGICA CONTINUA AQUI:
            // Se o diálogo estiver ativo, abortamos a execução e ignoramos o input de pausa
            if (dialogueSystem != null && dialogueSystem.IsDialogueActive)
            {
                return;
            }

            // Se não houver diálogo, controlamos a pausa normalmente
            if (isPaused)
            {
                ContinuarJogo();
            }
            else
            {
                PausarJogo();
            }
        }
    }

    public void PausarJogo()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true); // Mostra a tela de pause
        Time.timeScale = 0f;        // Congela o tempo do jogo (física, colisões, etc.)
    }

    public void ContinuarJogo()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false); // Esconde a tela de pause
        Time.timeScale = 1f;         // Normaliza o tempo do jogo
    }

    public void ReiniciarNivel()
    {
        // Garante que o tempo volta ao normal antes de recarregar a cena!
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VoltarAoMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}