using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // NOVO: Importa o sistema de controles moderno

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
        // NOVO: Usa a nossa função adaptada para o novo Input System
        if (WasAnyButtonPressed() && !isTransitioning)
        {
            AdvanceCutscene();
        }
    }

    // NOVO: Função que verifica o teclado, mouse e controle pelo novo sistema
    private bool WasAnyButtonPressed()
    {
        // 1. Verifica se qualquer tecla do teclado foi pressionada
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        // 2. Verifica se o botão esquerdo do mouse foi clicado
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        // 3. Verifica se o botão principal do controle (A no Xbox / X no PlayStation) foi apertado
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
            TransitionManager.Instance.DoTransition(() =>
            {
                displayImage.sprite = cutscenePanels[currentIndex];
            });

            Invoke(nameof(UnlockTransition), 2.5f);
        }
        else
        {
            TransitionManager.Instance.DoTransition(() =>
            {
                LoadNextScene();
            });
        }
    }

    private void UnlockTransition()
    {
        isTransitioning = false;
    }

    private void LoadNextScene()
    {
        Debug.Log($"[CutsceneManager] Carregando a cena: {nextSceneName}");

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[CutsceneManager] O nome da próxima cena não foi preenchido!");
        }
    }
}