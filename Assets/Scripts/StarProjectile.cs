using UnityEngine;

public class StarProjectile : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float lifeTime = 5f;
    public int damage = 1;

    private void Start()
    {
        // Destrói o projétil após alguns segundos para não pesar a memória caso erre o chão
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Move a estrela para baixo constantemente
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Lógica de colisão: Verifica se acertou o jogador ou o chão
        if (collision.CompareTag("Player"))
        {
            // Adicione a lógica para dar dano no jogador aqui
            collision.GetComponent<PlayerHealth>().TakeDamage(damage);

            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            // Opcional: Instanciar um efeito de impacto (partículas) aqui
            Destroy(gameObject);
        }
    }
}