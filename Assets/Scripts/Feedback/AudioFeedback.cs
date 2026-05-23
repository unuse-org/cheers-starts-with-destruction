using UnityEngine;

public class AudioFeedback : MonoBehaviour
{
    public enum SEType
    {
        Start,
        Break1,
        Break2,
        defeat,
        GameOver,
        CheersVoice
    }

    [Header("SE Clips")]
    [SerializeField] private AudioClip StartSE;
    [SerializeField] private AudioClip Break1SE;
    [SerializeField] private AudioClip Break2SE;
    [SerializeField] private AudioClip defeatSE;
    [SerializeField] private AudioClip GameOverSE;
    [SerializeField] private AudioClip[] CheersVoiceSEs;

    private AudioSource audioSource;

    // Singleton（推奨）
    public static AudioFeedback Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySE(SEType type)
    {
        AudioClip clip = GetClip(type);

        if (clip == null)
        {
            Debug.LogWarning($"[AudioFeedback] SE not set: {type}");
            return;
        }
        audioSource.PlayOneShot(clip);
    }

    public float PlayVoice(SEType type)
    {
        AudioClip clip = GetVoiceClip(type);
        return PlayVoiceClip(clip, type);
    }

    public AudioClip GetVoiceClip(SEType type)
    {
        return type == SEType.CheersVoice
            ? GetRandomCheersVoiceClip()
            : GetClip(type);
    }

    public float PlayVoiceClip(AudioClip clip, SEType type)
    {

        if (clip == null)
        {
            Debug.LogWarning($"[AudioFeedback] Voice SE not set: {type}");
            return 0f;
        }

        audioSource.PlayOneShot(clip);
        return clip.length;
    }

    public float GetClipLength(SEType type)
    {
        AudioClip clip = GetClip(type);
        return clip != null ? clip.length : 0f;
    }

    private AudioClip GetClip(SEType type)
    {
        switch (type)
        {
            case SEType.Start:
                return StartSE;
            case SEType.Break1:
                return Break1SE;
            case SEType.Break2:
                return Break2SE;
            case SEType.defeat:
                return defeatSE;
            case SEType.GameOver:
                return GameOverSE;
            case SEType.CheersVoice:
                return GetFirstCheersVoiceClip();
        }
        return null;
    }

    private AudioClip GetRandomCheersVoiceClip()
    {
        if (CheersVoiceSEs == null || CheersVoiceSEs.Length == 0)
            return null;

        int startIndex = Random.Range(0, CheersVoiceSEs.Length);
        for (int i = 0; i < CheersVoiceSEs.Length; i++)
        {
            AudioClip clip = CheersVoiceSEs[(startIndex + i) % CheersVoiceSEs.Length];
            if (clip != null)
                return clip;
        }

        return null;
    }

    private AudioClip GetFirstCheersVoiceClip()
    {
        if (CheersVoiceSEs == null) return null;

        for (int i = 0; i < CheersVoiceSEs.Length; i++)
        {
            if (CheersVoiceSEs[i] != null)
                return CheersVoiceSEs[i];
        }

        return null;
    }
}
