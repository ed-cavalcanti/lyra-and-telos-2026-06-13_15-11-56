using UnityEngine;
using UnityEngine.UI;
using System.Collections; // NOVO: Necessário para usar Coroutines

public class RuneManager : MonoBehaviour
{
    public static RuneManager Instance { get; private set; }

    [Header("Configurações da Interface (UI)")]
    [Tooltip("O GameObject pai (Painel) que segura todas as imagens das runas")]
    [SerializeField] private GameObject runeUIPanel; 
    [Tooltip("Tempo em segundos que a UI fica na tela antes de sumir")]
    [SerializeField] private float displayDuration = 3f;

    [Space]
    [Tooltip("Arraste as imagens das runas do Canvas na ordem (ex: Runa 1, Runa 2, Runa 3)")]
    [SerializeField] private Image[] runeUISlots;
    [SerializeField] private Color collectedColor = Color.white;
    [SerializeField] private Color uncollectedColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    [Header("Portal")]
    [SerializeField] private GameObject portal;

    private bool[] collectedRunes;
    private Coroutine hideUICoroutine; // Guarda a contagem atual para podermos resetá-la

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        collectedRunes = new bool[runeUISlots.Length];

        foreach (Image img in runeUISlots)
        {
            if (img != null) img.color = uncollectedColor;
        }

        // NOVO: Garante que o painel da UI comece invisível
        if (runeUIPanel != null) runeUIPanel.SetActive(false);

        if (portal != null) portal.SetActive(false);
    }

    public void CollectRune(int runeIndex)
    {
        if (runeIndex < 0 || runeIndex >= collectedRunes.Length) return;

        if (!collectedRunes[runeIndex])
        {
            collectedRunes[runeIndex] = true;
            
            if (runeUISlots[runeIndex] != null)
            {
                runeUISlots[runeIndex].color = collectedColor;
            }

            // NOVO: Mostra a UI e inicia o temporizador
            ShowUI();

            CheckWinCondition();
        }
    }

    private void ShowUI()
    {
        if (runeUIPanel != null)
        {
            runeUIPanel.SetActive(true); // Liga o painel

            // Se já tiver uma contagem regressiva rodando, nós a cancelamos
            // Isso evita que a UI suma muito rápido se o jogador pegar duas runas seguidas
            if (hideUICoroutine != null)
            {
                StopCoroutine(hideUICoroutine);
            }

            // Inicia uma nova contagem do zero
            hideUICoroutine = StartCoroutine(HideUIAfterDelay());
        }
    }

    // A Coroutine que conta o tempo
    private IEnumerator HideUIAfterDelay()
    {
        // Pausa a execução deste bloco pelo tempo determinado
        yield return new WaitForSeconds(displayDuration);
        
        // Desliga o painel após o tempo passar
        if (runeUIPanel != null)
        {
            runeUIPanel.SetActive(false);
        }
    }

    private void CheckWinCondition()
    {
        foreach (bool rune in collectedRunes)
        {
            if (!rune) return; 
        }

        Debug.Log("Todas as runas coletadas! Abrindo o portal...");
        
        if (portal != null)
        {
            portal.SetActive(true);
        }
    }
}