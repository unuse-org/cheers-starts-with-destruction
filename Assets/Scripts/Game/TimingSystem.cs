using System;
using UnityEngine;

namespace CheersGame.Game
{
    /// <summary>
    /// 乾杯タイミング判定を管理する。
    /// NPCController.OnCheersReady を受けて判定ウィンドウを開き、
    /// プレイヤーの入力タイミングに応じた連続スコア（0〜1）を返す。
    /// </summary>
    public class TimingSystem : MonoBehaviour
    {
        [Header("Timing Window")]
        [Tooltip("判定ウィンドウの総時間（秒）")]
        [SerializeField] private float _windowDuration = 2.0f;

        [Tooltip("Perfect ゾーン帯の半径（秒）— UI表示用")]
        [SerializeField] private float _perfectRadius = 0.15f;

        [Tooltip("Great ゾーン帯の半径（秒）— UI表示用")]
        [SerializeField] private float _greatRadius = 0.35f;

        [Tooltip("Good ゾーン帯の半径（秒）— この範囲外はスコア 0")]
        [SerializeField] private float _goodRadius = 0.6f;

        /// <summary>判定ウィンドウが開いているか</summary>
        public bool IsWindowOpen { get; private set; }

        /// <summary>
        /// ウィンドウの進行度（0 = 開始, 1 = 終了）。
        /// </summary>
        public float WindowProgress => IsWindowOpen
            ? Mathf.Clamp01((Time.time - _windowStartTime) / _windowDuration)
            : 0f;

        /// <summary>
        /// ジョッキの接近進行度（0 = 画面端, 1 = 中央衝突）。
        /// WindowProgress = 0.5 で 1 に達し、以降は 1 を保持する。
        /// </summary>
        public float GlassProgress => IsWindowOpen ? Mathf.Clamp01(WindowProgress * 2f) : 0f;

        /// <summary>判定ウィンドウがタイムアウトで閉じたときに発火</summary>
        public event Action OnWindowExpired;

        private float _windowStartTime;

        /// <summary>判定ウィンドウを開く。NPCController.OnCheersReady から呼ぶ。</summary>
        public void StartWindow()
        {
            StartWindow(_windowDuration);
        }

        /// <summary>
        /// 判定ウィンドウを指定した時間で開く。NPC の ReactionSpeed に応じた時間を渡す。
        /// </summary>
        public void StartWindow(float duration)
        {
            _windowDuration = duration;
            _windowStartTime = Time.time;
            IsWindowOpen = true;
            Debug.Log($"[TimingSystem] Window opened. Duration={duration:F2}s");
        }

        /// <summary>判定ウィンドウを明示的に閉じる。入力受付後に BattleManager から呼ぶ。</summary>
        public void CloseWindow()
        {
            IsWindowOpen = false;
        }

        /// <summary>
        /// 連続タイミングスコアを返す（0 = Miss, 1 = Perfect 中央）。
        /// 中央からの偏差が goodRadius 以上の場合は 0 を返す。
        /// </summary>
        public float GetTimingScore()
        {
            if (!IsWindowOpen) return 0f;

            float deviation = Mathf.Abs((Time.time - _windowStartTime) - _windowDuration * 0.5f);
            if (deviation >= _goodRadius) return 0f;
            return 1f - (deviation / _goodRadius);
        }

        private void Update()
        {
            if (IsWindowOpen && Time.time - _windowStartTime >= _windowDuration)
            {
                IsWindowOpen = false;
                Debug.Log("[TimingSystem] Window expired.");
                OnWindowExpired?.Invoke();
            }
        }
    }
}
