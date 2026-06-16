using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Instância estática que permite o acesso global (Singleton)
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public struct SoundEffect
    {
        public string name;       // Nome identificador do som (ex: "Pulo", "Moeda")
        public AudioClip clip;    // O arquivo de áudio propriamente dito
    }

    [Header("Configurações de Áudio")]
    [SerializeField] private AudioSource sfxSource;   // AudioSource dedicado aos efeitos sonoros
    [SerializeField] private AudioSource musicSource; // AudioSource dedicado às músicas de fundo

    [Header("Lista de Sons")]
    [SerializeField] private SoundEffect[] soundEffects; // Array com todos os seus SFX

    private void Awake()
    {
        // Garante que só exista um AudioManager no jogo
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destrói cópias duplicadas que possam surgir
        }
    }

    // Função para tocar Efeitos Sonoros (SFX) pelo Nome
    public void PlaySFX(string soundName)
    {
        // Procura o som na lista pelo nome informado
        SoundEffect s = Array.Find(soundEffects, item => item.name == soundName);

        if (s.clip != null)
        {
            // Toca o som uma vez (ideal para efeitos curtos)
            sfxSource.PlayOneShot(s.clip);
        }
        else
        {
            Debug.LogWarning("AudioManager: Som com o nome '" + soundName + "' não foi encontrado!");
        }
    }

    // Função para tocar Música de Fundo (BGM)
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true; // Músicas geralmente rodam em loop
            musicSource.Play();
        }
    }

    [Header("Sons Contínuos")]
    [SerializeField] private AudioSource loopingSfxSource; // O terceiro AudioSource do objeto

    public void PlayLoopingSFX(string soundName)
    {
        SoundEffect s = Array.Find(soundEffects, item => item.name == soundName);
        if (s.clip != null)
        {
            loopingSfxSource.clip = s.clip;
            loopingSfxSource.loop = true; // Mantém o som rodando enquanto carrega
            loopingSfxSource.Play();
        }
    }

    public void StopLoopingSFX()
    {
        loopingSfxSource.Stop();
    }
}