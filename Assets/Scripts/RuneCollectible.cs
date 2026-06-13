using UnityEngine;

public class RuneCollectible : MonoBehaviour
{
    [Header("Identificação")]
    [Tooltip("Qual runa é esta? 0 para a primeira, 1 para a segunda...")]
    public int runeID = 0;

    private void OnTriggerEnter2D(Collider2D col)
    {
        // Se for o jogador encostando na runa
        if (col.CompareTag("Player"))
        {
            // Avisa o Manager que esta runa específica foi pega
            RuneManager.Instance.CollectRune(runeID);

            // Efeito visual/sonoro opcional antes de sumir
            // Instantiate(coletaParticle, transform.position, Quaternion.identity);

            // Destrói o objeto da runa do mapa
            Destroy(gameObject);
        }
    }
}