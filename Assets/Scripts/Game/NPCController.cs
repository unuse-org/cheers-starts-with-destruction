using System;
using System.Collections;
using UnityEngine;
using CheersGame.Data;

namespace CheersGame.Game
{
    /// <summary>
    /// NPCの状態管理とカウントダウンシーケンスを担当する。
    /// GameManagerから Initialize → StartCheersSequence の順で呼ばれる。
    /// </summary>
    public class NPCController : MonoBehaviour
    {
        [SerializeField] private float _countdownInterval = 1.0f;

        public NPCData NPCData { get; private set; }

        /// <summary>カウントダウン完了時に発火</summary>
        public event Action OnCheersReady;

        /// <summary>カウント通知（3, 2, 1, 0）。0 = 乾杯!</summary>
        public event Action<int> OnCountdownTick;

        private Coroutine _countdownCoroutine;

        /// <summary>
        /// NPCデータで初期化する。
        /// </summary>
        public void Initialize(NPCData data)
        {
            NPCData = data;
            Debug.Log($"[NPCController] Initialized: {data.NPCName}");
        }

        /// <summary>
        /// カウントダウン（3, 2, 1, 乾杯!）を開始する。
        /// </summary>
        public void StartCheersSequence()
        {
            CancelCheersSequence();
            _countdownCoroutine = StartCoroutine(CountdownCoroutine());
        }

        /// <summary>
        /// カウントダウンを中断する。画面遷移時などに呼ぶ。
        /// </summary>
        public void CancelCheersSequence()
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }
        }

        private IEnumerator CountdownCoroutine()
        {
            for (int i = 3; i >= 1; i--)
            {
                Debug.Log($"[NPCController] Countdown: {i}");
                OnCountdownTick?.Invoke(i);
                yield return new WaitForSeconds(_countdownInterval);
            }

            Debug.Log("[NPCController] 乾杯!");
            OnCountdownTick?.Invoke(0);
            _countdownCoroutine = null;
            OnCheersReady?.Invoke();
        }
    }
}
