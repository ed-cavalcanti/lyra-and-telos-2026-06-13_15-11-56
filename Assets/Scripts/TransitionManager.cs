using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("Configurações")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private bool isTransitioning = false;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (fadeCanvasGroup != null && fadeCanvasGroup.gameObject != gameObject)
            {
                Destroy(fadeCanvasGroup.gameObject);
            }
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Garante que a tela sempre clareie e libere os cliques 
        // quando o jogo é aberto pela primeira vez no Menu Principal
        fadeCanvasGroup.blocksRaycasts = false;

        if (fadeCanvasGroup.alpha > 0f)
        {
            StartCoroutine(Fade(fadeCanvasGroup.alpha, 0f));
        }
    }

    // =========================================================
    // 1. PARA TELEPORTES (Mesma Cena)
    // =========================================================
    public void DoTransition(Action actionInTheDark)
    {
        if (isTransitioning) return;
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(TransitionCycleRoutine(actionInTheDark));
    }

    private IEnumerator TransitionCycleRoutine(Action actionInTheDark)
    {
        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;

        // FADE IN
        yield return Fade(0f, 1f);

        // EXECUTA A AÇÃO (Ex: Mudar o jogador de posição)
        actionInTheDark?.Invoke();
        yield return new WaitForSecondsRealtime(0.2f);

        // FADE OUT
        yield return Fade(1f, 0f);

        fadeCanvasGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    // =========================================================
    // 2. PARA MUDAR DE CENA (Com Carregamento Assíncrono)
    // =========================================================
    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;

        // FADE IN
        yield return Fade(0f, 1f);

        // INICIA O CARREGAMENTO NO MODO ASSÍNCRONO
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Espera pacientemente até que o Unity diga que a cena carregou 100%
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Dá 1 frame extra de respiro para que todos os Start() e Awake() da nova cena terminem
        yield return new WaitForEndOfFrame();

        // FADE OUT
        yield return Fade(1f, 0f);

        fadeCanvasGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    // =========================================================
    // FUNÇÃO AUXILIAR DE FADE
    // =========================================================
    private IEnumerator Fade(float startAlpha, float targetAlpha)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = targetAlpha;
    }
}