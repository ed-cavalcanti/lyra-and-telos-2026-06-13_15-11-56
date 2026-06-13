using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Transform heartsParent; // O painel/layout group que segura os corações
    [SerializeField] private GameObject heartPrefab; // O prefab de um coração na UI

    [Header("Sprites")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    private List<Image> heartImages = new List<Image>();

    private void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth não referenciado no HealthUI!");
            return;
        }

        // 1. Cria os corações na tela baseando-se na vida máxima
        InitializeHearts(playerHealth.GetMaxHealth());

        // 2. Inscreve os métodos nos eventos do seu script PlayerHealth
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
        // Atualiza a imagem de cada coração para cheio ou vazio
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
        // Aqui chamaremos a tela de Game Over futuramente
        Debug.Log("[UI] Preparando para exibir a tela de Game Over...");
    }
}