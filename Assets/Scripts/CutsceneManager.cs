using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Adicionado para carregar a cena caso o TransitionManager falhe

public class CutsceneManager : MonoBehaviour
{
    [Header("Imagens da Cutscene")]
    [Tooltip("Arraste as imagens (Sprites) na ordem em que devem aparecer")]
    [SerializeField] private Sprite[] cutscenePanels;

    [Tooltip("O componente de Imagem da UI onde a cutscene será exibida")]
    [SerializeField] private Image displayImage;

    [Header("Cena Seguinte")]
    [Tooltip("O nome exato da cena que deve carregar após a última imagem")]
    [SerializeField] private string nextSceneName;

    private int currentIndex = 0;
    private bool isTransitioning = false;

    private void Start()
    {
        if (cutscenePanels.Length > 0 && displayImage != null)
        {
            displayImage.sprite = cutscenePanels[0];
        }
    }

    private void Update()
    {
        if (WasAnyButtonPressed() && !isTransitioning)
        {
            AdvanceCutscene();
        }
    }

    private bool WasAnyButtonPressed()
    {
        // 1. Verifica teclado
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        // 2. Verifica mouse
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        // 3. Verifica controle (A / X)
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            return true;

        return false;
    }

    private void AdvanceCutscene()
    {
        isTransitioning = true;
        currentIndex++;

        if (currentIndex < cutscenePanels.Length)
        {
            // BLINDAGEM: Verifica se o TransitionManager existe para fazer a transição de imagens
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.DoTransition(() =>
                {
                    displayImage.sprite = cutscenePanels[currentIndex];
                });
            }
            else
            {
                Debug.LogWarning("[CutsceneManager] TransitionManager não encontrado! Trocando de imagem diretamente.");
                displayImage.sprite = cutscenePanels[currentIndex];
            }

            // Chama a função que destrava o clique após 2.5 segundos
            Invoke(nameof(UnlockTransition), 2.5f);
        }
        else
        {
            // Fim da cutscene: Carrega a próxima fase
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                // BLINDAGEM: Verifica se o TransitionManager existe para trocar de cena
                if (TransitionManager.Instance != null)
                {
                    TransitionManager.Instance.TransitionToScene(nextSceneName);
                }
                else
                {
                    Debug.LogWarning("[CutsceneManager] TransitionManager não encontrado! Carregando a cena diretamente.");
                    SceneManager.LoadScene(nextSceneName);
                }
            }
            else
            {
                Debug.LogWarning("[CutsceneManager] O nome da próxima cena não foi preenchido no Inspector!");
            }
        }
    }

    // Função que foi restaurada: ela permite que o jogador clique novamente
    private void UnlockTransition()
    {
        isTransitioning = false;
    }
}