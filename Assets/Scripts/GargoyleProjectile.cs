using UnityEngine;

public class GargoyleProjectile : MonoBehaviour
{
    [Header("Configurações do Projétil")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 4f; // Destrói o tiro após X segundos para não pesar na memória

    private Vector2 moveDirection;

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction;

        // Rotaciona o projétil para apontar na direção do movimento (útil se não for uma bola redonda)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Destrói automaticamente caso erre o alvo e voe pelo mapa
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Move o projétil todo frame na direção calculada
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log($"[Projétil] Bateu em: {col.gameObject.name} | Tag: {col.tag}");

        // Se bater no jogador
        if (col.CompareTag("Player"))
        {
            Debug.Log("[Projétil] Bateu no Player! Tentando achar o script PlayerHealth...");

            // Tenta pegar a vida no objeto que bateu
            if (col.TryGetComponent(out PlayerHealth health))
            {
                Debug.Log("[Projétil] Script PlayerHealth encontrado! Causando dano.");
                health.TakeDamage(damage, transform);
            }
            else
            {
                Debug.LogError($"[Projétil] ERRO: O objeto {col.gameObject.name} tem a tag 'Player', mas NÃO tem o script 'PlayerHealth' nele!");
            }

            Destroy(gameObject); // Destrói a bola mágica
        }
        // Destrói se bater no chão (opcional)
        else if (col.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}