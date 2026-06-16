using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Necessário para saber quando trocamos de cena

public class HealthUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform heartsParent;
    [SerializeField] private GameObject heartPrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    private PlayerHealth playerHealth; // Tiramos o SerializeField, o script acha sozinho agora!
    private List<Image> heartImages = new List<Image>();

    // Esta função liga o "radar" de troca de cena
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Esta função desliga o "radar"
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Tudo aqui dentro acontece no milissegundo em que a nova fase carrega
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Procura o novo jogador da fase atual
        playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning("[HealthUI] Nenhum PlayerHealth encontrado nesta cena!");
            return;
        }

        // 2. Limpa os corações antigos da fase anterior para não duplicar
        foreach (Transform child in heartsParent)
        {
            Destroy(child.gameObject);
        }
        heartImages.Clear();

        // 3. Recria a UI com a vida correta (Lembrando da variável estática que fizemos)
        InitializeHearts(playerHealth.GetMaxHealth());
        UpdateHearts(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());

        // 4. Reconecta os avisos de dano e morte no NOVO jogador
        playerHealth.OnHealthChanged.AddListener(UpdateHearts);
        playerHealth.OnDeath.AddListener(TriggerGameOverScreen);
    }

    private void InitializeHearts(int maxHealth)
    {
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heartObj = Instantiate(heartPrefab, heartsParent);
            Image heartImage = heartObj.GetComponent<Image>();
            heartImage.sprite = fullHeart;
            heartImages.Add(heartImage);
        }
    }

    private void UpdateHearts(int currentHealth, int maxHealth)
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentHealth)
                heartImages[i].sprite = fullHeart;
            else
                heartImages[i].sprite = emptyHeart;
        }
    }

    private void TriggerGameOverScreen()
    {
        // Aqui o seu GameOverManager entra em ação
        GameOverManager gameOver = FindAnyObjectByType<GameOverManager>();
        if (gameOver != null)
        {
            gameOver.ExibirGameOver();
        }
    }
}