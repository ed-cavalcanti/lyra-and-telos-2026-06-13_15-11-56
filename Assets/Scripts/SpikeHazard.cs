using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpikeHazard : MonoBehaviour
{
    [Header("Configuração de Dano")]
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

            HazardRespawn respawner = collision.GetComponent<HazardRespawn>();

            if (respawner != null)
            {
                respawner.TriggerHazardRespawn(damageAmount);
            }
            else
            {
            }
        }
    }
}