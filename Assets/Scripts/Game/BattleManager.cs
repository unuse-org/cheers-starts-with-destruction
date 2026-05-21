using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using CheersGame.Input;
using CheersGame.Data;

namespace CheersGame.Game
{
    public enum CheersResult
    {
        Victory, // 成功演出（被ダメージが少ない）
        Defeat,  // 失敗演出（被ダメージが大きい・時間切れ含む）
    }

    /// <summary>
    /// 乾杯バトルの判定・ダメージ処理を一元管理する。
    ///
    /// ダメージモデル:
    ///   damage = maxDamagePerCheers × (1 - timingScore)
    ///   Victory: damage < defeatDamageThreshold
    ///   Defeat : damage >= defeatDamageThreshold（時間切れ = 最大ダメージ）
    ///
    /// 処理フロー:
    ///   NPCController.OnCheersReady → TimingSystem.StartWindow()
    ///   ISensorInput.OnCheersDetected → Judge() → ResolveResult()
    ///   TimingSystem.OnWindowExpired → Defeat with full damage
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private NPCController _npcController;
        [SerializeField] private PlayerGlass _playerGlass;
        [SerializeField] private TimingSystem _timingSystem;
        [SerializeField] private MonoBehaviour _sensorInputComponent;

        [Header("Battle Config")]
        [Tooltip("1回の乾杯で受ける最大ダメージ。タイミングが中心から離れるほどこの値に近づく。")]
        [FormerlySerializedAs("_battlePower")]
        [SerializeField] private float _maxDamagePerCheers = 60f;

        [Tooltip("この値以上のダメージを受けたら Defeat 演出、それ未満なら Victory 演出。")]
        [SerializeField] private int _defeatDamageThreshold = 50;

        [Tooltip("基準ウィンドウ時間（秒）。NPC の ReactionSpeed で割って実際の時間を決定する。")]
        [SerializeField] private float _baseWindowDuration = 1.4f;

        [Header("Difficulty Scaling")]
        [Tooltip("この撃破数ごとにタイミングウィンドウを短くする。")]
        [SerializeField] private int _defeatsPerDifficultyStep = 5;

        [Tooltip("難易度が1段階上がるごとに短縮する秒数。")]
        [SerializeField] private float _windowDurationDecreasePerStep = 0.15f;

        [Tooltip("難易度上昇後もこれより短くしない基準ウィンドウ時間（秒）。")]
        [SerializeField] private float _minWindowDuration = 0.75f;

        [Tooltip("難易度が1段階上がるごとに増えるウィンドウ時間のランダム揺らぎ（±秒）。")]
        [SerializeField] private float _windowRandomnessPerStep = 0.05f;

        [Tooltip("ウィンドウ時間のランダム揺らぎ上限（±秒）。")]
        [SerializeField] private float _maxWindowRandomness = 0.15f;

        [Tooltip("結果表示後、次のNPCが登場するまでの待機時間（秒）")]
        [SerializeField] private float _resultDisplayDuration = 1.5f;

        /// <summary>タイミングスコア（0〜1）を通知</summary>
        public event Action<float> OnTimingJudged;

        /// <summary>乾杯結果を通知</summary>
        public event Action<CheersResult> OnCheersResolved;

        private ISensorInput _sensorInput;

        private void Awake()
        {
            _sensorInput = _sensorInputComponent as ISensorInput;
            if (_sensorInput == null)
                Debug.LogError("[BattleManager] SensorInputComponent does not implement ISensorInput.");
        }

        private void OnEnable()
        {
            if (_sensorInput != null)
            {
                _sensorInput.OnCheersDetected += HandleCheersDetected;
            }
            if (_npcController != null)
                _npcController.OnCheersReady += HandleCheersReady;
            if (_timingSystem != null)
                _timingSystem.OnWindowExpired += HandleWindowExpired;
        }

        private void OnDisable()
        {
            if (_sensorInput != null)
            {
                _sensorInput.OnCheersDetected -= HandleCheersDetected;
            }
            if (_npcController != null)
                _npcController.OnCheersReady -= HandleCheersReady;
            if (_timingSystem != null)
                _timingSystem.OnWindowExpired -= HandleWindowExpired;
        }

        //アニメーター取得
        private Animator GetCurrentAnimator()
        {
            if (_npcController == null)
            {
                Debug.Log("NPCController is NULL");
                return null;
            }

            if (_npcController.CharacterView == null)
            {
                Debug.Log("CharacterView is NULL");
                return null;
            }

            Animator animator =
                _npcController.CharacterView.GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.Log("Animator NOT FOUND");
            }
            else
            {
                Debug.Log($"Animator FOUND : {animator.name} / controller : {animator.runtimeAnimatorController?.name ?? "NULL"}");
            }

            return animator;
        }

        private void HandleCheersDetected(CheersInputData data)
        {
            if (_gameManager == null || _gameManager.CurrentState != GameState.Game) return;
            if (_gameManager.CurrentNPC == null) return;
            if (!_timingSystem.IsWindowOpen) return;

            float timingScore = _timingSystem.GetTimingScore();
            _timingSystem.CloseWindow();

            int damage = CalculateDamage(timingScore);
            CheersResult result = damage >= _defeatDamageThreshold
                ? CheersResult.Defeat : CheersResult.Victory;

            Debug.Log($"[BattleManager] timing={timingScore:F2} damage={damage} threshold={_defeatDamageThreshold} → {result}");

            OnTimingJudged?.Invoke(timingScore);
            OnCheersResolved?.Invoke(result);

            ResolveResult(result, damage);
        }

        private int CalculateDamage(float timingScore)
        {
            float normalizedDamage = 1f - Mathf.Clamp01(timingScore);
            return Mathf.RoundToInt(Mathf.Max(0f, _maxDamagePerCheers) * normalizedDamage);
        }

        private void HandleWindowExpired()
        {
            if (_gameManager == null || _gameManager.CurrentState != GameState.Game) return;

            int damage = Mathf.RoundToInt(Mathf.Max(0f, _maxDamagePerCheers));
            OnTimingJudged?.Invoke(0f);
            OnCheersResolved?.Invoke(CheersResult.Defeat);
            ResolveResult(CheersResult.Defeat, damage);
        }

        private void ResolveResult(CheersResult result, int damage)
        {
            if (damage > 0)
            {
                _playerGlass.TakeDamage(damage);
                LogHP(result.ToString(), damage);
            }

            // 死亡確認時にSE再生
            if (_playerGlass.IsBroken)
            {
                AudioFeedback.Instance.PlaySE(AudioFeedback.SEType.GameOver);
                return;
            }

            Animator animator = GetCurrentAnimator();
            NPCData npc = _gameManager.CurrentNPC;

            switch (result)
            {
                case CheersResult.Victory:
                    AudioFeedback.Instance.PlaySE(AudioFeedback.SEType.Break1);
                    TryPlayState(animator, npc?.AnimStateWin);
                    _gameManager.AddDefeat();
                    StartCoroutine(SpawnNextNPCAfterDelay());
                    break;

                case CheersResult.Defeat:
                    AudioFeedback.Instance.PlaySE(AudioFeedback.SEType.defeat);
                    TryPlayState(animator, npc?.AnimStateLose);
                    StartCoroutine(ContinueCheersAfterDelay());
                    break;
            }
        }
        private void HandleCheersReady()
        {
            NPCData npc = _gameManager.CurrentNPC;
            Animator animator = GetCurrentAnimator();
            TryPlayState(animator, npc?.AnimStateCheers);
            float reactionSpeed = Mathf.Max(0.01f, npc != null ? npc.ReactionSpeed : 1.0f);
            int difficultyStep = GetDifficultyStep();
            float randomOffset;
            float baseDuration = GetCurrentBaseWindowDuration(difficultyStep, out randomOffset);
            float duration = baseDuration / reactionSpeed;

            Debug.Log($"[Difficulty] defeats={_gameManager.DefeatCount} step={difficultyStep} baseWindow={baseDuration:F2}s random={randomOffset:+0.00;-0.00;0.00}s reaction={reactionSpeed:F2} actualWindow={duration:F2}s");
            _timingSystem.StartWindow(duration);
        }

        private int GetDifficultyStep()
        {
            int defeatsPerStep = Mathf.Max(1, _defeatsPerDifficultyStep);
            int defeats = _gameManager != null ? _gameManager.DefeatCount : 0;
            return defeats / defeatsPerStep;
        }

        private float GetCurrentBaseWindowDuration(int difficultyStep, out float randomOffset)
        {
            float steppedDuration = _baseWindowDuration - (_windowDurationDecreasePerStep * difficultyStep);
            steppedDuration = Mathf.Max(_minWindowDuration, steppedDuration);

            float randomness = Mathf.Min(_maxWindowRandomness, _windowRandomnessPerStep * difficultyStep);
            randomOffset = randomness > 0f ? UnityEngine.Random.Range(-randomness, randomness) : 0f;

            return Mathf.Max(_minWindowDuration, steppedDuration + randomOffset);
        }

        private void TryPlayState(Animator animator, string stateName)
        {
            if (animator == null || string.IsNullOrEmpty(stateName)) return;
            int hash = Animator.StringToHash(stateName);
            if (!animator.HasState(0, hash))
            {
                Debug.LogWarning($"[BattleManager] state not found: '{stateName}' / controller: {animator.runtimeAnimatorController?.name ?? "NULL"}");
                return;
            }
            animator.Play(stateName);
        }

        private void LogHP(string context, int damage)
        {
            int current = _playerGlass.CurrentDurability;
            int max     = _playerGlass.GlassData != null ? _playerGlass.GlassData.MaxDurability : 0;
            Debug.Log($"[HP] {context} -({damage}) → 残 {current}/{max}");
        }

        private IEnumerator SpawnNextNPCAfterDelay()
        {
            yield return new WaitForSeconds(_resultDisplayDuration);
            _gameManager.SpawnNextNPC();
        }

        private IEnumerator ContinueCheersAfterDelay()
        {
            yield return new WaitForSeconds(_resultDisplayDuration);
            if (_playerGlass.IsBroken) yield break;
            Animator animator = GetCurrentAnimator();
            NPCData npc = _gameManager.CurrentNPC;
            TryPlayState(animator, npc?.AnimStateLoseLoop);
            _npcController.StartCheersSequence();
        }
    }
}
