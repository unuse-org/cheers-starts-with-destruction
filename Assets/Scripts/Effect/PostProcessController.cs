using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using CheersGame.Game; 

public class PostProcessController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume _postProcessVolume;
    [SerializeField] private PlayerGlass _playerGlass; 

    [Header("Threshold Settings")]
    [Tooltip("このHP割合を下回ったらエフェクトが変化し始めます")]
    [SerializeField, Range(0f, 1f)] private float _effectHpThreshold = 0.5f;

    [Header("Effect Settings")]
    [SerializeField] private float _baseChromaticIntensity = 0.0f; 
    [SerializeField] private float _maxChromaticIntensity = 0.5f;  

    [Space(10)]
    [SerializeField] private float _baseVignetteIntensity = 0.0f;  
    [SerializeField] private float _maxVignetteIntensity = 0.5f;   

    // ▼ 追加：周期的に変化させるスピード
    [Header("Pulse Settings")]
    [Tooltip("色収差が脈打つ速さ（数値が大きいほど速い）")]
    [SerializeField] private float _chromaticPulseSpeed = 5.0f;

    private ChromaticAberration _chromaticAberration;
    private Vignette _vignette;

    // 現在のHP割合を保持しておく変数
    private float _currentHpRatio = 1.0f;

    void Awake()
    {
        if (_postProcessVolume != null && _postProcessVolume.profile != null)
        {
            _postProcessVolume.profile.TryGet(out _chromaticAberration);
            _postProcessVolume.profile.TryGet(out _vignette);
        }
    }

    private void Start()
    {
        if (_playerGlass != null)
        {
            // 初期状態のHP割合を取得
            _currentHpRatio = _playerGlass.DurabilityRatio;
        }
    }

    private void OnEnable()
    {
        if (_playerGlass != null)
        {
            _playerGlass.OnDurabilityChanged += OnDurabilityChanged;
        }
    }

    private void OnDisable()
    {
        if (_playerGlass != null)
        {
            _playerGlass.OnDurabilityChanged -= OnDurabilityChanged;
        }
    }

    // ▼ 変更：HPが変わった時は変数に保存するだけにする
    private void OnDurabilityChanged(int currentHp)
    {
        if (_playerGlass == null) return;
        _currentHpRatio = _playerGlass.DurabilityRatio;
    }

    // ▼ 追加：毎フレームエフェクトを計算して適用する
    private void Update()
    {
        float targetChromatic = _baseChromaticIntensity;
        float targetVignette = _baseVignetteIntensity;

        if (_currentHpRatio <= _effectHpThreshold)
        {
            // 閾値からの進行度 (0.0 ～ 1.0)
            float damageProgress = Mathf.InverseLerp(_effectHpThreshold, 0f, _currentHpRatio);
            
            // 現在のHPにおける色収差の「本来の強さ」を計算
            float maxCurrentChromatic = Mathf.Lerp(_baseChromaticIntensity, _maxChromaticIntensity, damageProgress);
            
            // サイン波を使って 0.0 ～ 1.0 の間を滑らかに上下する波を作る
            float wave = (Mathf.Sin(Time.time * _chromaticPulseSpeed) + 1f) / 2f;

            // 波の値を使って、初期値と現在の強さの間を周期的に変化させる
            targetChromatic = Mathf.Lerp(_baseChromaticIntensity, maxCurrentChromatic, wave);
            
            // ビネットは周期変化させず、ダメージ進行度に合わせて一定の強さにする
            targetVignette = Mathf.Lerp(_baseVignetteIntensity, _maxVignetteIntensity, damageProgress);
        }

        ChangeChromaticAberration(targetChromatic);
        ChangeVignetteIntensity(targetVignette);
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