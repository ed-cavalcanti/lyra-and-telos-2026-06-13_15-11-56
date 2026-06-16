using UnityEngine;

public class SceneMusicStarter : MonoBehaviour
{
    [Header("Configuração de Áudio")]
    [Tooltip("Arraste o arquivo de áudio (música) para este campo")]
    [SerializeField] private AudioClip musicClip; // Mudamos de string para AudioClip

    private void Start()
    {
        // Verifica se o arquivo de áudio foi associado antes de tocar
        if (musicClip != null)
        {
            // Agora passamos o arquivo real de áudio para a sua função
            AudioManager.Instance.PlayMusic(musicClip);
            Debug.Log($"[SceneMusicStarter] Iniciando a música: {musicClip.name}");
        }
        else
        {
            Debug.LogWarning("[SceneMusicStarter] O AudioClip da música não foi arrastado no Inspector!");
        }
    }
}