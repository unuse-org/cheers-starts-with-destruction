using System;
using UnityEngine;
using CheersGame.Input;
using CheersGame.Data;

namespace CheersGame.Game
{
    public enum CheersResult
    {
        Whiff,        // スカ（タイミング外）
        Victory,      // 勝利（NPC撃破）
        Defeat,       // 敗北（自グラスにダメージ）
        Draw,         // 引き分け（NPC撃破 + 自グラスに小ダメージ）
        SelfDestruct, // 自爆（角度不正）
    }

    /// <summary>
    /// 乾杯バトルの判定・ダメージ処理を一元管理する。
    ///
    /// 処理フロー:
    ///   NPCController.OnCheersReady → TimingSystem.StartWindow()
    ///   ISensorInput.OnCheersDetected → JudgeCheers() → ResolveResult()
    ///   TimingSystem.OnWindowExpired → StartCheersSequence()（入力なし時）
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
        [Tooltip("タイミング判定なしの基礎攻撃力")]
        [SerializeField] private float _baseAttackPower = 50f;

        [Tooltip("この角度（絶対値）を超えると自爆になる（度）")]
        [SerializeField] private float _badAngleThreshold = 45f;

        [Tooltip("Defeat 時のプレイヤーへのダメージ")]
        [SerializeField] private int _defeatDamage = 20;

        [Tooltip("Draw 時のプレイヤーへの小ダメージ")]
        [SerializeField] private int _drawDamage = 5;

        [Tooltip("SelfDestruct 時のプレイヤーへの大ダメージ")]
        [SerializeField] private int _selfDestructDamage = 40;

        /// <summary>タイミング判定結果を通知（UIフィードバック用）</summary>
        public event Action<TimingGrade> OnTimingJudged;

        /// <summary>乾杯結果を通知（UIフィードバック用）</summary>
        public event Action<CheersResult> OnCheersResolved;

        private ISensorInput _sensorInput;
        private VoiceInputData _lastVoiceData;

        private void Awake()
        {
            _sensorInput = _sensorInputComponent as ISensorInput;
            if (_sensorInput == null)
            {
                Debug.LogError("[BattleManager] SensorInputComponent does not implement ISensorInput.");
            }
        }

        private void OnEnable()
        {
            if (_sensorInput != null)
            {
                _sensorInput.OnCheersDetected += HandleCheersDetected;
                _sensorInput.OnVoiceDetected += HandleVoiceDetected;
            }

            if (_npcController != null)
            {
                _npcController.OnCheersReady += HandleCheersReady;
            }

            if (_timingSystem != null)
            {
                _timingSystem.OnWindowExpired += HandleWindowExpired;
            }
        }

        private void OnDisable()
        {
            if (_sensorInput != null)
            {
                _sensorInput.OnCheersDetected -= HandleCheersDetected;
                _sensorInput.OnVoiceDetected -= HandleVoiceDetected;
            }

            if (_npcController != null)
            {
                _npcController.OnCheersReady -= HandleCheersReady;
            }

            if (_timingSystem != null)
            {
                _timingSystem.OnWindowExpired -= HandleWindowExpired;
            }
        }

        private void HandleCheersReady()
        {
            _timingSystem.StartWindow();
            Debug.Log("[BattleManager] Timing window opened.");
        }

        private void HandleVoiceDetected(VoiceInputData data)
        {
            _lastVoiceData = data;
        }

        private void HandleCheersDetected(CheersInputData data)
        {
            if (_gameManager == null || _gameManager.CurrentState != GameState.Game) return;
            if (_gameManager.CurrentNPC == null) return;
            if (!_timingSystem.IsWindowOpen) return;

            TimingGrade timing = _timingSystem.JudgeTiming();
            CheersResult result = JudgeCheers(data, _lastVoiceData, _gameManager.CurrentNPC, timing);

            _timingSystem.CloseWindow();

            Debug.Log($"[BattleManager] Timing={timing}, Result={result}");
            OnTimingJudged?.Invoke(timing);
            OnCheersResolved?.Invoke(result);

            ResolveResult(result);
        }

        private void HandleWindowExpired()
        {
            if (_gameManager == null || _gameManager.CurrentState != GameState.Game) return;
            Debug.Log("[BattleManager] Window expired - Defeat.");
            OnTimingJudged?.Invoke(TimingGrade.Miss);
            OnCheersResolved?.Invoke(CheersResult.Defeat);
            _playerGlass.TakeDamage(_defeatDamage);
            if (!_playerGlass.IsBroken)
                _npcController.StartCheersSequence();
        }

        private CheersResult JudgeCheers(
            CheersInputData input,
            VoiceInputData voice,
            NPCData npc,
            TimingGrade timing)
        {
            if (Mathf.Abs(input.Angle) >= _badAngleThreshold)
                return CheersResult.SelfDestruct;

            if (timing == TimingGrade.Miss)
                return CheersResult.Whiff;

            float attackPower = CalculateAttackPower(timing, voice.Volume);
            float defense = npc.DefenseThreshold;

            if (attackPower > defense * 1.2f) return CheersResult.Victory;
            if (attackPower > defense * 0.8f) return CheersResult.Draw;
            return CheersResult.Defeat;
        }

        private float CalculateAttackPower(TimingGrade timing, float voiceVolume)
        {
            float timingMultiplier = timing switch
            {
                TimingGrade.Perfect => 1.5f,
                TimingGrade.Great   => 1.2f,
                TimingGrade.Good    => 1.0f,
                _                  => 0.0f,
            };
            float voiceBonus = 1.0f + (voiceVolume * 0.5f);
            return _baseAttackPower * timingMultiplier * voiceBonus;
        }

        private void ResolveResult(CheersResult result)
        {
            switch (result)
            {
                case CheersResult.Victory:
                    _gameManager.AddDefeat();
                    _gameManager.SpawnNextNPC();
                    break;

                case CheersResult.Draw:
                    _playerGlass.TakeDamage(_drawDamage);
                    _gameManager.AddDefeat();
                    if (!_playerGlass.IsBroken)
                        _gameManager.SpawnNextNPC();
                    break;

                case CheersResult.Defeat:
                    _playerGlass.TakeDamage(_defeatDamage);
                    if (!_playerGlass.IsBroken)
                        _npcController.StartCheersSequence();
                    break;

                case CheersResult.SelfDestruct:
                    _playerGlass.TakeDamage(_selfDestructDamage);
                    if (!_playerGlass.IsBroken)
                        _npcController.StartCheersSequence();
                    break;

                case CheersResult.Whiff:
                    _npcController.StartCheersSequence();
                    break;
            }
        }
    }
}
