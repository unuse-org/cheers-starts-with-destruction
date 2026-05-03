using System;
using System.Collections;
using UnityEngine;
using CheersGame.Input;
using CheersGame.Data;

namespace CheersGame.Game
{
    public enum CheersResult
    {
        Victory, // 勝利（NPC撃破）
        Defeat,  // 敗北（タイミング外・時間切れ含む）
    }

    /// <summary>
    /// 乾杯バトルの判定・ダメージ処理を一元管理する。
    ///
    /// ダメージモデル:
    ///   attackPower = battlePower × timingScore (× voiceBonus if _useVoice)
    ///   damage      = battlePower - attackPower  (常にプレイヤーに適用)
    ///   Victory: attackPower >= npc.DefenseThreshold
    ///   Defeat : それ以外（時間切れ = timingScore 0 = full damage）
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
        [Tooltip("attack + damage = この値（タイミングで両者が連続的に変化する）")]
        [SerializeField] private float _battlePower = 100f;

        [Tooltip("基準ウィンドウ時間（秒）。NPC の ReactionSpeed で割って実際の時間を決定する。")]
        [SerializeField] private float _baseWindowDuration = 2.0f;

        [Tooltip("Perfect 中央でのタイミング倍率上限")]
        [SerializeField] private float _maxTimingMultiplier = 1.5f;

        [Tooltip("結果表示後、次のNPCが登場するまでの待機時間（秒）")]
        [SerializeField] private float _resultDisplayDuration = 1.5f;

        [Header("Voice Bonus (Optional)")]
        [Tooltip("声の大きさによる攻撃ボーナスを使用するか")]
        [SerializeField] private bool _useVoice = false;
        [Tooltip("声量ボーナスのスケール（useVoice ON 時のみ有効）")]
        [SerializeField] private float _voiceBonusScale = 0.5f;

        /// <summary>タイミングスコア（0〜1）を通知</summary>
        public event Action<float> OnTimingJudged;

        /// <summary>乾杯結果を通知</summary>
        public event Action<CheersResult> OnCheersResolved;

        private ISensorInput _sensorInput;
        private VoiceInputData _lastVoiceData;

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
                _sensorInput.OnVoiceDetected  += HandleVoiceDetected;
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
                _sensorInput.OnVoiceDetected  -= HandleVoiceDetected;
            }
            if (_npcController != null)
                _npcController.OnCheersReady -= HandleCheersReady;
            if (_timingSystem != null)
                _timingSystem.OnWindowExpired -= HandleWindowExpired;
        }

        private void HandleCheersReady()
        {
            float reactionSpeed = _gameManager.CurrentNPC != null
                ? _gameManager.CurrentNPC.ReactionSpeed : 1.0f;
            _timingSystem.StartWindow(_baseWindowDuration / reactionSpeed);
        }

        private void HandleVoiceDetected(VoiceInputData data) => _lastVoiceData = data;

        private void HandleCheersDetected(CheersInputData data)
        {
            if (_gameManager == null || _gameManager.CurrentState != GameState.Game) return;
            if (_gameManager.CurrentNPC == null) return;
            if (!_timingSystem.IsWindowOpen) return;

            float timingScore = _timingSystem.GetTimingScore();
            _timingSystem.CloseWindow();

            float voiceBonus  = _useVoice ? 1f + (_lastVoiceData.Volume * _voiceBonusScale) : 1f;
            float attackPower = Mathf.Min(_battlePower, _battlePower * timingScore * _maxTimingMultiplier * voiceBonus);
            int   damage      = Mathf.RoundToInt(Mathf.Max(0f, _battlePower - attackPower));

            CheersResult result = attackPower >= _gameManager.CurrentNPC.DefenseThreshold
                ? CheersResult.Victory : CheersResult.Defeat;

            Debug.Log($"[BattleManager] timing={timingScore:F2} attack={attackPower:F1} damage={damage} → {result}");

            OnTimingJudged?.Invoke(timingScore);
            OnCheersResolved?.Invoke(result);

            ResolveResult(result, damage);
        }

        private void HandleWindowExpired()
        {
            if (_gameManager == null || _gameManager.CurrentState != GameState.Game) return;

            int damage = Mathf.RoundToInt(_battlePower);
            OnTimingJudged?.Invoke(0f);
            OnCheersResolved?.Invoke(CheersResult.Defeat);

            _playerGlass.TakeDamage(damage);
            LogHP("時間切れ", damage);
            if (!_playerGlass.IsBroken)
                _npcController.StartCheersSequence();
        }

        private void ResolveResult(CheersResult result, int damage)
        {
            if (damage > 0)
            {
                _playerGlass.TakeDamage(damage);
                LogHP(result.ToString(), damage);
            }

            if (_playerGlass.IsBroken) return;

            switch (result)
            {
                case CheersResult.Victory:
                    _gameManager.AddDefeat();
                    StartCoroutine(SpawnNextNPCAfterDelay());
                    break;

                case CheersResult.Defeat:
                    _npcController.StartCheersSequence();
                    break;
            }
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
    }
}
