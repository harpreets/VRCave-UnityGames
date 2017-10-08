using UnityEngine;
using System.Collections;

public enum SoundEffects { ObstacleCollisionSoundEffect, CoinSoundEffect, PowerUpSoundEffect, GameOverSoundEffect, GUITapSoundEffect }
public class AudioManager : MonoBehaviour {

    static public AudioManager instance;

    public AudioClip backgroundMusic;
    public AudioClip obstacleCollision;
    public AudioClip coinCollection;
    public AudioClip powerUpCollection;
    public AudioClip gameOver;
    public AudioClip guiTap;

    public float backgroundMusicVolume;
    public float soundEffectsVolume;

    private AudioSource backgroundAudio;
    private AudioSource soundEffectsAudio;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        AudioSource[] sources = Camera.main.GetComponents<AudioSource>();
        backgroundAudio = sources[0];
        soundEffectsAudio = sources[1];

        

        backgroundAudio.clip = backgroundMusic;
        backgroundAudio.loop = true;
        backgroundAudio.volume = Mathf.Clamp01(backgroundMusicVolume);

        soundEffectsAudio.volume = Mathf.Clamp01(soundEffectsVolume);
    }

    public void playBackgroundMusic(bool play)
    {
        if (play) {
            backgroundAudio.Play();
        } else {
            backgroundAudio.Pause();
        }
    }

    public void playSoundEffect(SoundEffects soundEffect)
    {
        AudioClip clip = null;
        float pitch = 1;
        switch (soundEffect) {
            case SoundEffects.ObstacleCollisionSoundEffect:
                clip = obstacleCollision;
                break;

            case SoundEffects.CoinSoundEffect:
                clip = coinCollection;
                pitch = 1.5f;
                break;

            case SoundEffects.PowerUpSoundEffect:
                clip = powerUpCollection;
                break;

            case SoundEffects.GameOverSoundEffect:
                clip = gameOver;
                break;

            case SoundEffects.GUITapSoundEffect:
                clip = guiTap;
                break;
        }

        soundEffectsAudio.pitch = pitch;
        soundEffectsAudio.clip = clip;
        soundEffectsAudio.Play();
    }
}
