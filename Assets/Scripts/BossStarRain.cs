using System.Collections;
using UnityEngine;

public class BossStarRain : MonoBehaviour
{
    [Header("Configurações do Ataque")]
    public GameObject starPrefab;
    public float attackDuration = 4f;
    public float spawnInterval = 0.2f;

    [Header("Área de Spawn")]
    [Tooltip("Se verdadeiro, as estrelas caem de uma altura fixa no mapa. Se falso, a altura acompanha o Boss.")]
    public bool useFixedWorldHeight = true;
    public float spawnHeight = 10f;          // Se a opção acima for True, este valor será a posição Y exata no seu mapa (ex: o teto da arena)
    public float spawnAreaWidth = 10f;

    public enum BossState { Idle, Attacking, Dead }
    // O BossController gerencia o estado, este script apenas executa a chuva

    // Coroutine chamada pelo BossController
    public IEnumerator CastStarRain()
    {
        float timer = 0f;

        while (timer < attackDuration)
        {
            SpawnStar();
            timer += spawnInterval;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnStar()
    {
        AudioManager.Instance.PlaySFX("ProjectileGo");
        // Calcula a variação horizontal (X) aleatória
        float randomX = Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);

        // CORREÇÃO 1: Define se a altura Y usa a coordenada absoluta do mapa ou se acompanha o Boss
        float targetY = useFixedWorldHeight ? spawnHeight : transform.position.y + spawnHeight;

        // Posição final onde o projétil vai nascer
        Vector2 spawnPosition = new Vector2(transform.position.x + randomX, targetY);

        // CORREÇÃO 2: Aplica a rotação de 90 graus no eixo Z. 
        // Se o sprite ficar de ponta cabeça, mude o valor para -90f ou 270f.
        Quaternion projectileRotation = Quaternion.Euler(0f, 0f, -90f);

        // Instancia aplicando a nova posição e a nova rotação
        Instantiate(starPrefab, spawnPosition, projectileRotation);
    }

    // Atualizado para desenhar o indicador visual na altura correta no Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        float targetY = useFixedWorldHeight ? spawnHeight : transform.position.y + spawnHeight;
        Vector3 center = new Vector3(transform.position.x, targetY, 0);
        Vector3 size = new Vector3(spawnAreaWidth, 0.5f, 0);
        Gizmos.DrawWireCube(center, size);
    }
}