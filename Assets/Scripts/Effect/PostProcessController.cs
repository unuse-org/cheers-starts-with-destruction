using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using CheersGame.Game; 

public class PostProcessController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume _postProcessVolume;
    [SerializeField] private PlayerGlass _playerGlass; 

    // ▼ 変更：インスペクターから最小値・最大値を自由に設定できるようにしました
    [Header("Effect Settings")]
    [SerializeField] private float _baseChromaticIntensity = 0.0f; // 平常時（HP満タン）の色収差
    [SerializeField] private float _maxChromaticIntensity = 0.5f;    // 瀕死時（HPゼロ）の最大色収差

    [Space(10)]
    [SerializeField] private float _baseVignetteIntensity = 0.0f;  // 平常時のビネット（暗さ）
    [SerializeField] private float _maxVignetteIntensity = 0.5f;     // 瀕死時の最大ビネット

    private ChromaticAberration _chromaticAberration;
    private Vignette _vignette;

    void Awake()
    {
        if (_postProcessVolume != null && _postProcessVolume.profile != null)
        {
            _postProcessVolume.profile.TryGet(out _chromaticAberration);
            _postProcessVolume.profile.TryGet(out _vignette);
        }
    }

    private void OnEnable()
    {
        if (_playerGlass != null)
        {
            _playerGlass.OnDurabilityChanged += UpdatePostProcess;
        }
    }

    private void OnDisable()
    {
        if (_playerGlass != null)
        {
            _playerGlass.OnDurabilityChanged -= UpdatePostProcess;
        }
    }

    private void UpdatePostProcess(int currentHp)
    {
        if (_playerGlass == null) return;

        // damageLevel は 0.0 (無傷) ～ 1.0 (HP0) の値になります
        float damageLevel = 1.0f - _playerGlass.DurabilityRatio;
        
        // ▼ 変更：Mathf.Lerp を使用して、Base(無傷) と Max(瀕死) の間を damageLevel の割合で計算
        float newChromatic = Mathf.Lerp(_baseChromaticIntensity, _maxChromaticIntensity, damageLevel);
        float newVignette = Mathf.Lerp(_baseVignetteIntensity, _maxVignetteIntensity, damageLevel);

        ChangeChromaticAberration(newChromatic);
        ChangeVignetteIntensity(newVignette);
    }

    public void ChangeChromaticAberration(float intensityValue)
    {
        if (_chromaticAberration != null)
        {
            _chromaticAberration.intensity.value = intensityValue;
        }
    }

    public void ChangeVignetteIntensity(float intensityValue)
    {
        if (_vignette != null)
        {
            _vignette.intensity.value = intensityValue;
        }
    }
}