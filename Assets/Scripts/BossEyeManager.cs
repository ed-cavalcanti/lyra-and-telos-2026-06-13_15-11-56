using UnityEngine;

public class BossEyeManager : MonoBehaviour
{
    [Header("Configurações dos Olhos")]
    public Transform[] eyes;
    public GameObject projectilePrefab;

    [Header("Configurações de Disparo Aleatório")]
    [Tooltip("Tempo mínimo que um olho espera para atirar de novo")]
    public float minTimeBetweenShots = 1.5f;
    [Tooltip("Tempo máximo que um olho espera para atirar de novo")]
    public float maxTimeBetweenShots = 4f;

    // Array para guardar o cronômetro individual de cada olho
    private float[] eyeTimers;

    private Transform player;
    private BossController bossController;

    private void Start()
    {
        bossController = GetComponent<BossController>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Inicializa o array de cronômetros com o mesmo tamanho da lista de olhos
        eyeTimers = new float[eyes.Length];

        // Define um tempo inicial aleatório para cada olho.
        // O valor mínimo é 0.5f para que eles não atirem no exato milissegundo que a luta começar.
        for (int i = 0; i < eyes.Length; i++)
        {
            eyeTimers[i] = Random.Range(0.5f, maxTimeBetweenShots);
        }
    }

    private void Update()
    {
        // NOVO: Adicionamos o "BossState.Attacking" na lista de bloqueio.
        // Agora os olhos não atiram se o boss estiver Morto, Vulnerável (roxo) OU Atacando (Chuva).
        if (player == null || bossController == null ||
            bossController.currentState == BossController.BossState.Dead ||
            bossController.currentState == BossController.BossState.Vulnerable ||
            bossController.currentState == BossController.BossState.Attacking)
        {
            return;
        }

        // Verifica o cronômetro de CADA olho de forma independente
        for (int i = 0; i < eyes.Length; i++)
        {
            if (eyes[i] == null || !eyes[i].gameObject.activeInHierarchy) continue;

            eyeTimers[i] -= Time.deltaTime;

            if (eyeTimers[i] <= 0f)
            {
                ShootFromSingleEye(eyes[i]);
                eyeTimers[i] = Random.Range(minTimeBetweenShots, maxTimeBetweenShots);
            }
        }
    }

    private void ShootFromSingleEye(Transform eye)
    {
        AudioManager.Instance.PlaySFX("GargoyleProjectile");
        // Instancia o projétil
        GameObject proj = Instantiate(projectilePrefab, eye.position, eye.rotation);

        // Calcula a direção rumo ao jogador
        Vector2 direction = (player.position - eye.position).normalized;

        // Passa a direção para o script do projétil inimigo
        if (proj.TryGetComponent(out EnemyProjectile enemyProj))
        {
            enemyProj.SetDirection(direction);
        }
    }
}