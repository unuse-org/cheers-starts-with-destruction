using UnityEngine;

public class AudioFeedback : MonoBehaviour
{
    public enum SEType
    {
        Start,
        Break1,
        Break2,
        Break3,
        Break4
    }

    [Header("SE Clips")]
    [SerializeField] private AudioClip StartSE;
    [SerializeField] private AudioClip Break1SE;
    [SerializeField] private AudioClip Break2SE;
    [SerializeField] private AudioClip Break3SE;
    [SerializeField] private AudioClip Break4SE;

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
            case SEType.Break3:
                return Break3SE;
            case SEType.Break4:
                return Break4SE;
        }
        return null;
    }
}