using System.Collections;
using UnityEngine;

public class TemporaryPlatform : MonoBehaviour
{
    [Header("Configurações de Tempo")]
    [SerializeField] private float tempoParaDestruir = 2f;
    [SerializeField] private float tempoParaRenascer = 3f;

    private Animator animator;
    private Collider2D plataformaCollider;
    private bool playerEmCima = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        plataformaCollider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Detecta o player por cima
        if (collision.gameObject.CompareTag("Player") && !playerEmCima)
        {
            if (collision.contacts[0].normal.y < -0.5f)
            {
                StartCoroutine(PlatformSequency());
            }
        }
    }

    private IEnumerator PlatformSequency()
    {
        playerEmCima = true;

        // 1. Espera o tempo do player em cima da plataforma
        yield return new WaitForSeconds(tempoParaDestruir);

        // 2. Manda rodar a animação de destruir
        animator.SetTrigger("Destroy");

        // 3. Desativa o colisor IMEDIATAMENTE para o player cair
        plataformaCollider.enabled = false;

        // 4. Espera o tempo em que ela deve ficar sumida + tempo para renascer
        yield return new WaitForSeconds(tempoParaRenascer);

        // 5. Manda rodar a animação de regenerar
        animator.SetTrigger("Regenerate");
    }

    // Método público que chamaremos de dentro da animação de Regenerate para ligar a colisão de volta
    public void ReactivateColission()
    {
        plataformaCollider.enabled = true;
        playerEmCima = false;
    }
}