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

        _controller.SetMovementLocked(true); // O Player agora pausa a gravidade sozinho!

        _health.TakeDamage(damage, null);

        if (_health.GetCurrentHealth() <= 0)
        {
            _isRespawning = false;
            yield break;
        }

        yield return new WaitForSeconds(respawnDelay);

        Vector2 safePos = _controller.GetSafePosition();

        // Como o Rigidbody continua "simulado", esta linha não vai mais causar crash!
        _controller.TeleportTo(safePos);

        _controller.SetMovementLocked(false);
        _isRespawning = false;
    }
}