using UnityEngine;

public class BossEye : MonoBehaviour
{
    private Transform player;

    [Tooltip("Ajuste este valor se o olho ficar de lado ou de costas para o player (ex: 90, -90, 180)")]
    public float rotationOffset = 0f;

    private void Start()
    {
        // Encontra o jogador automaticamente pela Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Calcula o vetor de direção do olho até o jogador
        Vector2 direction = player.position - transform.position;

        // Converte a direção para um ângulo em graus
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Aplica a rotação no eixo Z (2D)
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }
}