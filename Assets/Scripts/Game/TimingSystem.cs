using System;
using UnityEngine;

namespace CheersGame.Game
{
    public enum TimingGrade
    {
        Perfect,
        Great,
        Good,
        Miss,
    }

    /// <summary>
    /// 乾杯タイミング判定を管理する。
    /// NPCController.OnCheersReady を受けて判定ウィンドウを開き、
    /// プレイヤーの入力タイミングに応じて TimingGrade を返す。
    /// </summary>
    public class TimingSystem : MonoBehaviour
    {
        [Header("Timing Window")]
        [Tooltip("判定ウィンドウの総時間（秒）")]
        [SerializeField] private float _windowDuration = 2.0f;

        [Tooltip("Perfect 判定の中心からのズレ許容（秒）")]
        [SerializeField] private float _perfectRadius = 0.15f;

        [Tooltip("Great 判定の中心からのズレ許容（秒）")]
        [SerializeField] private float _greatRadius = 0.35f;

        [Tooltip("Good 判定の中心からのズレ許容（秒）")]
        [SerializeField] private float _goodRadius = 0.6f;

        /// <summary>判定ウィンドウが開いているか</summary>
        public bool IsWindowOpen { get; private set; }

        /// <summary>
        /// ウィンドウの進行度（0 = 開始, 1 = 終了）。
        /// インジケーターUI のアニメーションに使用する。
        /// </summary>
        public float WindowProgress => IsWindowOpen
            ? Mathf.Clamp01((Time.time - _windowStartTime) / _windowDuration)
            : 0f;

        /// <summary>判定ウィンドウがタイムアウトで閉じたときに発火</summary>
        public event Action OnWindowExpired;

        private float _windowStartTime;

        /// <summary>判定ウィンドウを開く。NPCController.OnCheersReady から呼ぶ。</summary>
        public void StartWindow()
        {
            _windowStartTime = Time.time;
            IsWindowOpen = true;
            Debug.Log("[TimingSystem] Window opened.");
        }

        /// <summary>判定ウィンドウを明示的に閉じる。入力受付後に BattleManager から呼ぶ。</summary>
        public void CloseWindow()
        {
            IsWindowOpen = false;
        }

        /// <summary>
        /// 現在時刻でタイミング判定を行う。
        /// ウィンドウが閉じていれば Miss を返す。
        /// </summary>
        public TimingGrade JudgeTiming()
        {
            if (!IsWindowOpen) return TimingGrade.Miss;

            float elapsed = Time.time - _windowStartTime;
            float center = _windowDuration * 0.5f;
            float deviation = Mathf.Abs(elapsed - center);

            if (deviation <= _perfectRadius) return TimingGrade.Perfect;
            if (deviation <= _greatRadius)   return TimingGrade.Great;
            if (deviation <= _goodRadius)    return TimingGrade.Good;
            return TimingGrade.Miss;
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
