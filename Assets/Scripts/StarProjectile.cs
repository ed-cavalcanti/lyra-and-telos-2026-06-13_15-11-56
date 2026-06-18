using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Força a Unity a não esquecer do Rigidbody
public class StarProjectile : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float lifeTime = 5f;
    public int damage = 1;

    private Rigidbody2D rb;

    private void Start()
    {
        // Pega o componente de física da estrela
        rb = GetComponent<Rigidbody2D>();

        // Em vez de usar Update para mover, damos uma velocidade constante para a física resolver!
        if (rb != null)
        {
            rb.linearVelocity = Vector2.down * fallSpeed;
        }

        // Destrói o projétil após alguns segundos
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("A estrela bateu no objeto: " + collision.gameObject.name + " | Layer: " + LayerMask.LayerToName(collision.gameObject.layer));

        // Lógica de colisão: Verifica se acertou o jogador
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerHealth>().TakeDamage(damage, transform);
            Destroy(gameObject);
        }
    }
}