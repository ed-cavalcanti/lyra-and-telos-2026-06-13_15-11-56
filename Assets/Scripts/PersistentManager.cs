using UnityEngine;

public class PersistentManager : MonoBehaviour
{
    public static PersistentManager Instance { get; private set; }

    private void Awake()
    {
        // Se já existe um _Managers salvo da fase anterior...
        if (Instance != null && Instance != this)
        {
            // ...destrói o _Managers vazio que veio na cena nova!
            Destroy(gameObject);
            return;
        }

        // Se for o primeiro _Managers do jogo, protege ele e todos os filhos
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}