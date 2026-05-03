using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CheersGame.Data;
using CheersGame.Game;

namespace CheersGame.Feedback
{
    /// <summary>
    /// バトル結果・ダメージ・NPC変化などのゲームイベントに対して
    /// 視覚的フィードバックを提供する。
    ///
    /// 既存クラスのイベントを購読するだけで動作するため、
    /// BattleManager / PlayerGlass / GameManager への変更は不要。
    ///
    /// Inspector 設定：
    ///   - _battleManager, _playerGlass, _gameManager を配線する
    ///   - _mainCamera に Main Camera を設定する
    ///   - _glassCrackOverlay に GameScreen 内の全画面 Image を設定する
    ///   - _npcGlassShardsParticle / _playerGlassShardsParticle は任意（null 時スキップ）
    /// </summary>
    public class VisualFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BattleManager _battleManager;
        [SerializeField] private PlayerGlass _playerGlass;
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private ParticleSystem _npcGlassShardsParticle;
        [SerializeField] private ParticleSystem _playerGlassShardsParticle;
        [SerializeField] private Image _glassCrackOverlay;

        [Header("Screen Shake")]
        [Tooltip("Defeat / Victory 時の揺れ時間（秒）")]
        [SerializeField] private float _shakeDurationSmall = 0.5f;
        [Tooltip("Defeat / Victory 時の揺れ振幅（Unity 単位）")]
        [SerializeField] private float _shakeAmplitudeSmall = 20f;

        [Tooltip("Draw 時の揺れ時間（秒）")]
        [SerializeField] private float _shakeDurationMedium = 0.3f;
        [Tooltip("Draw 時の揺れ振幅")]
        [SerializeField] private float _shakeAmplitudeMedium = 5f;

        [Tooltip("SelfDestruct / グラス破壊時の揺れ時間（秒）")]
        [SerializeField] private float _shakeDurationLarge = 0.50f;
        [Tooltip("SelfDestruct / グラス破壊時の揺れ振幅")]
        [SerializeField] private float _shakeAmplitudeLarge = 10f;

        private Vector3 _cameraOriginalLocalPosition;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            if (_mainCamera != null)
            {
                _cameraOriginalLocalPosition = _mainCamera.transform.localPosition;
            }
        }

        private void OnEnable()
        {
            if (_battleManager != null)
            {
                _battleManager.OnCheersResolved += HandleCheersResolved;
            }

            if (_playerGlass != null)
            {
                _playerGlass.OnDurabilityChanged += HandleDurabilityChanged;
                _playerGlass.OnGlassBroken += HandleGlassBroken;
            }

            if (_gameManager != null)
            {
                _gameManager.OnNPCChanged += HandleNPCChanged;
                _gameManager.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (_battleManager != null)
            {
                _battleManager.OnCheersResolved -= HandleCheersResolved;
            }

            if (_playerGlass != null)
            {
                _playerGlass.OnDurabilityChanged -= HandleDurabilityChanged;
                _playerGlass.OnGlassBroken -= HandleGlassBroken;
            }

            if (_gameManager != null)
            {
                _gameManager.OnNPCChanged -= HandleNPCChanged;
                _gameManager.OnStateChanged -= HandleStateChanged;
            }
        }

        // ── イベントハンドラ ──────────────────────────────────────────

        private void HandleCheersResolved(CheersResult result)
        {
            switch (result)
            {
                case CheersResult.Victory:
                    PlayParticle(_npcGlassShardsParticle);
                    TriggerShake(_shakeDurationSmall, _shakeAmplitudeSmall);
                    break;

                case CheersResult.Defeat:
                    TriggerShake(_shakeDurationMedium, _shakeAmplitudeMedium);
                    break;
            }
        }

        private void HandleDurabilityChanged(int currentDurability)
        {
            if (_glassCrackOverlay == null) return;
            if (_playerGlass == null || _playerGlass.GlassData == null) return;

            int maxDurability = _playerGlass.GlassData.MaxDurability;
            if (maxDurability <= 0) return;

            float damagedRatio = 1f - (float)currentDurability / maxDurability;
            float alpha = damagedRatio * 0.2f;
            _glassCrackOverlay.color = new Color(1f, 0.2f, 0.2f, alpha);
        }

        private void HandleGlassBroken()
        {
            Debug.Log("[VF] Glass broken!");
            PlayParticle(_playerGlassShardsParticle);
            TriggerShake(_shakeDurationLarge, _shakeAmplitudeLarge);

            if (_glassCrackOverlay != null)
            {
                _glassCrackOverlay.color = new Color(1f, 0.2f, 0.2f, 0.2f);
            }
        }

        private void HandleNPCChanged(NPCData npcData)
        {
            Debug.Log($"[VF] NPC appeared: {npcData?.NPCName}");
            // アニメーター実装後にNPC登場演出を追加予定
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.Game)
            {
                ResetCrackOverlay();
            }
        }

        // ── ヘルパー ─────────────────────────────────────────────────

        private void PlayParticle(ParticleSystem ps)
        {
            if (ps == null) return;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }

        private void TriggerShake(float duration, float amplitude)
        {
            if (_mainCamera == null) return;

            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _mainCamera.transform.localPosition = _cameraOriginalLocalPosition;
            }

            _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, amplitude));
        }

        private IEnumerator ShakeRoutine(float duration, float amplitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * amplitude;
                float y = Random.Range(-1f, 1f) * amplitude;
                _mainCamera.transform.localPosition = _cameraOriginalLocalPosition + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _mainCamera.transform.localPosition = _cameraOriginalLocalPosition;
            _shakeCoroutine = null;
        }

        private void ResetCrackOverlay()
        {
            if (_glassCrackOverlay == null) return;
            _glassCrackOverlay.color = new Color(1f, 0.2f, 0.2f, 0f);
        }
    }
}
