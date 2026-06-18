using System.Collections;
using UnityEngine;
using TarodevController;

[RequireComponent(typeof(PlayerController), typeof(PlayerHealth))]
public class HazardRespawn : MonoBehaviour
{
    private PlayerController _controller;
    private PlayerHealth _health;
    private bool _isRespawning;

    [Header("Configurações do Hazard")]
    [SerializeField] private float respawnDelay = 0.4f;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _health = GetComponent<PlayerHealth>();
    }

    public void TriggerHazardRespawn(int damage)
    {
        if (_isRespawning)
        {
            return;
        }

        StartCoroutine(RespawnRoutine(damage));
    }

    private IEnumerator RespawnRoutine(int damage)
    {
        _isRespawning = true;

        // Trava o movimento do jogador
        _controller.SetMovementLocked(true);

        // Aplica o dano
        _health.TakeDamage(damage, null);

        // Se esse dano for o suficiente para matar o jogador:
        if (_health.GetCurrentHealth() <= 0)
        {
            // CORREÇÃO: Destrava o movimento antes de cancelar a rotina!
            // Assim, ao clicar em "Continuar" no Game Over, o jogador volta a andar.
            _controller.SetMovementLocked(false);
            _isRespawning = false;

            yield break; // Encerra a rotina do espinho (O Game Over assume daqui)
        }

        // Se o jogador sobreviveu ao dano:
        yield return new WaitForSeconds(respawnDelay);

        Vector2 safePos = _controller.GetSafePosition();

        _controller.TeleportTo(safePos);

        // Destrava o movimento após o teleporte seguro
        _controller.SetMovementLocked(false);
        _isRespawning = false;
    }
}