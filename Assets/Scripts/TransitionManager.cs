using UnityEngine;
using System.Collections;
using System; // Necessário para passarmos ações (callbacks)

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("Configurações")]
    [Tooltip("Arraste o Canvas Group da tela preta aqui")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [Tooltip("Tempo em segundos que o Fade leva para acontecer")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool fadeFromBlackOnStart = true; // NOVO CAMPO

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // NOVO LÓGICA DE START
        if (fadeFromBlackOnStart)
        {
            // Trava a tela no preto absoluto e bloqueia cliques
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;

            // Chama a sua própria função para clarear a tela
            FadeFromBlack();
        }
        else
        {
            // Comportamento original: começa com a tela limpa imediatamente
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    // --------------------------------------------------------
    // FUNÇÕES PÚBLICAS PARA VOCÊ CHAMAR EM QUALQUER SCRIPT
    // --------------------------------------------------------

    /// Escurece a tela até ficar 100% preta
    public void FadeToBlack()
    {
        StartCoroutine(FadeRoutine(1f));
    }

    /// Clareia a tela até sumir o preto
    public void FadeFromBlack()
    {
        StartCoroutine(FadeRoutine(0f));
    }

    /// Faz o ciclo completo: Escurece, Executa seu código, e Clareia de volta.
    private bool isTransitioning = false; // Trava de segurança para impedir múltiplas transições

    public void DoTransition(Action actionInTheDark)
    {
        // Se já estiver no meio de uma transição, ignora os novos chamados
        if (isTransitioning) return;

        StartCoroutine(TransitionCycleRoutine(actionInTheDark));
    }

    // --------------------------------------------------------
    // LÓGICA INTERNA (COROUTINES)
    // --------------------------------------------------------

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        // Bloqueia cliques do jogador enquanto a tela estiver preta
        fadeCanvasGroup.blocksRaycasts = true;

        float startAlpha = fadeCanvasGroup.alpha;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeDuration);
            yield return null; // Espera o próximo frame
        }

        fadeCanvasGroup.alpha = targetAlpha;

        // Se a tela clareou totalmente, libera os cliques do mouse
        if (targetAlpha == 0f)
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator TransitionCycleRoutine(Action actionInTheDark)
    {
        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;

        // 1. FADE IN (Escurecer)
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            // Time.unscaledDeltaTime ignora qualquer pausa ou slowdown no jogo
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null; // Espera o processamento do próximo frame
        }

        fadeCanvasGroup.alpha = 1f; // Garante que cravou em 100%

        // O SEGREDO AQUI: Obriga o Unity a desenhar a tela preta no monitor ANTES de continuar o código
        yield return new WaitForEndOfFrame();

        // 2. EXECUTA O TELEPORTE (Agora sim, no escuro absoluto)
        actionInTheDark?.Invoke();

        // 3. Pausa de segurança no escuro para garantir que a câmera e física se estabilizaram
        yield return new WaitForSecondsRealtime(0.2f);

        // 4. FADE OUT (Clarear)
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        isTransitioning = false; // Libera a trava para futuras transições
    }
}