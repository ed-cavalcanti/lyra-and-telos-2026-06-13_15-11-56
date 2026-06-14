using UnityEngine;

public class EnemyTouchDamage : MonoBehaviour
{
    [Header("Configurações de Dano")]
    [SerializeField] private int touchDamage = 1;
    [SerializeField] private float damageCooldown = 1f;

    private float nextDamageTime;

    // Usamos Stay para que o jogador tome dano contínuo se decidir ficar encostado
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Verifica se o tempo de recarga já passou
        if (Time.time >= nextDamageTime)
        {
            // Tenta pegar o script PlayerHealth no objeto que colidiu
            if (collision.gameObject.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(touchDamage, transform);

                // Reinicia o cronômetro do cooldown
                nextDamageTime = Time.time + damageCooldown;

                Debug.Log("[Contato] Causou dano no player ao encostar!");
            }
        }
    }
}