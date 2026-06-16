using UnityEngine;
using UnityEngine.UI;

public class RuneManager : MonoBehaviour
{
    public static RuneManager Instance { get; private set; }

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
    // === NOVO: Ponto de teleporte final ===
    [SerializeField] private Transform teleportPointFinal;

    [Header("Referências")]
    [SerializeField] private Transform playerTransform;

    // Variáveis Estáticas (Sobrevivem à troca de cena)
    private static bool[] globalCollectedRunes;
    private static int globalTotalRunesCollected = 0;
    private static bool isMemoryInitialized = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (!isMemoryInitialized)
        {
            globalCollectedRunes = new bool[runeUISlots.Length];
            globalTotalRunesCollected = 0;
            isMemoryInitialized = true;
        }

        for (int i = 0; i < runeUISlots.Length; i++)
        {
            if (runeUISlots[i] != null)
            {
                runeUISlots[i].color = globalCollectedRunes[i] ? collectedColor : uncollectedColor;
            }
        }

        if (runeUIPanel != null) runeUIPanel.SetActive(true);

        if (portalPhase1 != null) portalPhase1.SetActive(false);
        if (finalPortal != null) finalPortal.SetActive(false);
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
                TransitionManager.Instance.DoTransition(() =>
                {
                    playerTransform.position = teleportPointPhase2.position;
                    CortarCameraPara(teleportPointPhase2.position);
                });
            }
        }
        // O jogador pegou as 5 runas (Fase 2)
        else if (globalTotalRunesCollected == globalCollectedRunes.Length)
        {
            Debug.Log("Todas as 5 runas coletadas! Abrindo o portal final...");

            if (finalPortal != null) finalPortal.SetActive(true);

            // === NOVO: LÓGICA DO TELEPORTE FINAL ===
            if (playerTransform != null && teleportPointFinal != null)
            {
                TransitionManager.Instance.DoTransition(() =>
                {
                    playerTransform.position = teleportPointFinal.position;
                    CortarCameraPara(teleportPointFinal.position);
                });
            }
        }
    }

    // Função criada para organizar o código e não repetir a lógica da câmera duas vezes
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

        // Verifica se o jogador já pegou todas as runas (caso raro, mas previne bugs)
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
}