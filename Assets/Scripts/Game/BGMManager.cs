using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public enum GameState
    {
        Title,
        Game,
        Score
    }

    [Header("BGM Clips")]
    public AudioClip titleBGM;
    public AudioClip gameBGM;
    public AudioClip scoreBGM;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void ChangeBGM(GameState state)
    {
        AudioClip nextClip = GetClip(state);
        PlayBGM(nextClip);
    }

    private AudioClip GetClip(GameState state)
    {
        switch (state)
        {
            case GameState.Title:
                return titleBGM;
            case GameState.Game:
                return gameBGM;
            case GameState.Score:
                return scoreBGM;
        }
        return null;
    }

    private void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // 🔥 ここが重要（AudioClipで判定）
        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            return;
        }

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
}