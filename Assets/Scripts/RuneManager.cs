using UnityEngine;
using UnityEngine.UI;

public class RuneManager : MonoBehaviour
{
    public static RuneManager Instance { get; private set; }

    [Header("Configurações da Cena")]
    [Tooltip("Marque na cena do Boss para esconder as runas da tela")]
    [SerializeField] private bool ocultarInterfaceNestaCena = false;

    [Header("Configurações da Interface (UI)")]
    [SerializeField] private GameObject runeUIPanel;
    [SerializeField] private Image[] runeUISlots;
    [SerializeField] private Color collectedColor = Color.white;
    [SerializeField] private Color uncollectedColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    [Header("Fase 1 (Transição)")]
    [SerializeField] private int runesToFinishPhase1 = 3;
    [SerializeField] private GameObject portalPhase1;
    [SerializeField] private Transform teleportPointPhase2;

    [Header("Fase 2 (Fim)")]
    [SerializeField] private GameObject finalPortal;
    [SerializeField] private Transform teleportPointFinal;

    [Header("Referências")]
    [SerializeField] private Transform playerTransform;

    // Variáveis Estáticas (Sobrevivem à troca de cena)
    private static bool[] globalCollectedRunes;
    private static int globalTotalRunesCollected = 0;
    private static bool isMemoryInitialized = false;

    private void Awake()
    {
        // 1. SINGLETON LOCAL: A Instância sempre será o Manager da cena ATUAL.
        Instance = this;

        // 2. Inicializa OU expande a memória das runas na memória estática
        if (!isMemoryInitialized)
        {
            globalCollectedRunes = new bool[runeUISlots.Length];
            globalTotalRunesCollected = 0;
            isMemoryInitialized = true;
            Debug.Log($"[RuneManager] Inicializado com {runeUISlots.Length} slots");
        }
        // Se a fase seguinte tem mais slots, redimensiona o array mantendo os dados
        else if (globalCollectedRunes != null && globalCollectedRunes.Length < runeUISlots.Length)
        {
            Debug.Log($"[RuneManager] Redimensionando runas de {globalCollectedRunes.Length} para {runeUISlots.Length} slots");
            bool[] novaMemoria = new bool[runeUISlots.Length];
            for (int i = 0; i < globalCollectedRunes.Length; i++)
            {
                novaMemoria[i] = globalCollectedRunes[i];
            }
            globalCollectedRunes = novaMemoria;
        }

        // 3. Atualiza os ícones na NOVA tela com proteção anti-crash
        for (int i = 0; i < runeUISlots.Length; i++)
        {
            if (runeUISlots[i] != null && globalCollectedRunes != null)
            {
                bool foiColetada = (i < globalCollectedRunes.Length) ? globalCollectedRunes[i] : false;
                runeUISlots[i].color = foiColetada ? collectedColor : uncollectedColor;
            }
        }

        // 4. Decide se a UI já começa ligada ou desligada nesta cena
        if (ocultarInterfaceNestaCena)
        {
            EsconderInterface();
        }
        else
        {
            MostrarInterface();
        }

        if (portalPhase1 != null) portalPhase1.SetActive(false);
        if (finalPortal != null) finalPortal.SetActive(false);
    }

    public void EsconderInterface()
    {
        if (runeUIPanel != null) runeUIPanel.SetActive(false);
    }

    public void MostrarInterface()
    {
        if (runeUIPanel != null) runeUIPanel.SetActive(true);
    }

    public void CollectRune(int runeIndex)
    {
        if (runeIndex < 0 || runeIndex >= globalCollectedRunes.Length) return;

        if (!globalCollectedRunes[runeIndex])
        {
            globalCollectedRunes[runeIndex] = true;
            globalTotalRunesCollected++;

            if (runeUISlots[runeIndex] != null)
            {
                runeUISlots[runeIndex].color = collectedColor;
            }

            CheckProgression();
        }
    }

    private void CheckProgression()
    {
        // SEGURANÇA: Se a referência do jogador foi perdida na troca de cena, busca o novo jogador!
        if (playerTransform == null)
        {
            PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
            if (player != null) playerTransform = player.transform;
        }

        // O jogador pegou as 3 runas (Fase 1)
        if (globalTotalRunesCollected == runesToFinishPhase1)
        {
            Debug.Log("Fase 1 concluída! Ativando portal 1 e teleportando...");

            if (portalPhase1 != null) portalPhase1.SetActive(true);

            if (playerTransform != null && teleportPointPhase2 != null)
            {
                // BLINDAGEM ANTI-BUG: Verifica se o TransitionManager realmente existe na cena de teste
                if (TransitionManager.Instance != null)
                {
                    TransitionManager.Instance.DoTransition(() =>
                    {
                        playerTransform.position = teleportPointPhase2.position;
                        CortarCameraPara(teleportPointPhase2.position);
                    });
                }
                else
                {
                    // Caso falte o TransitionManager, faz o teleporte seco sem quebrar o jogo
                    Debug.LogWarning("[RuneManager] TransitionManager não encontrado nesta cena! Teleportando diretamente de forma seca.");
                    playerTransform.position = teleportPointPhase2.position;
                    CortarCameraPara(teleportPointPhase2.position);
                }
            }
        }
        // O jogador pegou as 5 runas (Fase 2)
        else if (globalTotalRunesCollected == globalCollectedRunes.Length)
        {
            Debug.Log("Todas as 5 runas coletadas! Abrindo o portal final...");

            if (finalPortal != null) finalPortal.SetActive(true);

            // LÓGICA DO TELEPORTE FINAL
            if (playerTransform != null && teleportPointFinal != null)
            {
                // BLINDAGEM ANTI-BUG para a Fase 2 também
                if (TransitionManager.Instance != null)
                {
                    TransitionManager.Instance.DoTransition(() =>
                    {
                        playerTransform.position = teleportPointFinal.position;
                        CortarCameraPara(teleportPointFinal.position);
                    });
                }
                else
                {
                    Debug.LogWarning("[RuneManager] TransitionManager não encontrado nesta cena! Teleportando para o final de forma seca.");
                    playerTransform.position = teleportPointFinal.position;
                    CortarCameraPara(teleportPointFinal.position);
                }
            }
        }
    }

    private void CortarCameraPara(Vector3 novaPosicao)
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(novaPosicao.x, novaPosicao.y, mainCam.transform.position.z);
            Behaviour cinemachineBrain = (Behaviour)mainCam.GetComponent("CinemachineBrain");
            if (cinemachineBrain != null)
            {
                cinemachineBrain.enabled = false;
                cinemachineBrain.enabled = true;
            }
        }
    }

    public void RegistrarPortalFinal(GameObject portal, Transform teleportPoint)
    {
        finalPortal = portal;
        teleportPointFinal = teleportPoint;

        if (globalTotalRunesCollected == globalCollectedRunes.Length)
        {
            if (finalPortal != null) finalPortal.SetActive(true);
        }
        else
        {
            if (finalPortal != null) finalPortal.SetActive(false);
        }

        Debug.Log("RuneManager: Portal Final conectado com sucesso na nova cena!");
    }

    public static void ResetarMemoriaDasRunas()
    {
        if (globalCollectedRunes != null)
        {
            for (int i = 0; i < globalCollectedRunes.Length; i++)
            {
                globalCollectedRunes[i] = false;
            }
        }
        globalTotalRunesCollected = 0;
        isMemoryInitialized = false;
    }
}