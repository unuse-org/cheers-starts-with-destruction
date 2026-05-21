using UnityEngine;

public class AudioFeedback : MonoBehaviour
{
    public enum SEType
    {
        Start,
        Break1,
        Break2,
        defeat,
        GameOver
    }

    [Header("SE Clips")]
    [SerializeField] private AudioClip StartSE;
    [SerializeField] private AudioClip Break1SE;
    [SerializeField] private AudioClip Break2SE;
    [SerializeField] private AudioClip defeatSE;
    [SerializeField] private AudioClip GameOverSE;

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
        }
        return null;
    }
}