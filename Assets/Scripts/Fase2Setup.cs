using UnityEngine;

public class Fase2Setup : MonoBehaviour
{
    [Header("Objetos desta Cena")]
    [SerializeField] private GameObject portalFinalDestaFase;
    [SerializeField] private Transform teleporteFinalDestaFase;

    private void Start()
    {
        // Assim que a Fase 2 começa, ele procura o RuneManager que veio da Fase 1
        // e entrega os objetos novos para ele!
        if (RuneManager.Instance != null)
        {
            RuneManager.Instance.RegistrarPortalFinal(portalFinalDestaFase, teleporteFinalDestaFase);
        }
        else
        {
            Debug.LogWarning("Fase2Setup: O RuneManager não foi encontrado!");
        }
    }
}