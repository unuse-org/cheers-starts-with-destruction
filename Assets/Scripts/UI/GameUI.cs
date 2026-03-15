using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CheersGame.Data;
using CheersGame.Game;

namespace CheersGame.UI
{
    /// <summary>
    /// ゲーム画面のUI制御。
    /// 耐久値バー・撃破数・グラス名・カウントダウン・タイミングガイド・結果表示を担当する。
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private PlayerGlass _playerGlass;
        [SerializeField] private NPCController _npcController;
        [SerializeField] private BattleManager _battleManager;
        [SerializeField] private TimingSystem _timingSystem;

        [Header("Durability")]
        [SerializeField] private Slider _durabilitySlider;
        [SerializeField] private TextMeshProUGUI _durabilityText;

        [Header("Game Info")]
        [SerializeField] private TextMeshProUGUI _defeatCountText;
        [SerializeField] private TextMeshProUGUI _glassNameText;

        [Header("NPC Info")]
        [SerializeField] private TextMeshProUGUI _npcNameText;

        [Header("Countdown")]
        [SerializeField] private TextMeshProUGUI _countdownText;

        [Header("Timing Guide")]
        [Tooltip("タイミングガイド全体のパネル。ウィンドウ開閉に合わせて表示/非表示する。")]
        [SerializeField] private GameObject _timingGuidePanel;
        [Tooltip("タイミングバー全体の RectTransform（インジケーターの移動範囲の基準）")]
        [SerializeField] private RectTransform _timingBarRect;
        [Tooltip("移動するインジケーターの RectTransform")]
        [SerializeField] private RectTransform _timingIndicator;

        [Header("Result")]
        [SerializeField] private TextMeshProUGUI _resultText;
        [Tooltip("結果テキストを表示する秒数")]
        [SerializeField] private float _resultDisplayDuration = 1.5f;

        private Coroutine _resultClearCoroutine;
        private bool _timingGuideVisible;

        private void OnEnable()
        {
            if (_playerGlass != null)
                _playerGlass.OnDurabilityChanged += HandleDurabilityChanged;

            if (_gameManager != null)
            {
                _gameManager.OnDefeatCountChanged += HandleDefeatCountChanged;
                _gameManager.OnNPCChanged += HandleNPCChanged;
            }

            if (_npcController != null)
                _npcController.OnCountdownTick += HandleCountdownTick;

            if (_battleManager != null)
            {
                _battleManager.OnTimingJudged += HandleTimingJudged;
                _battleManager.OnCheersResolved += HandleCheersResolved;
            }

            RefreshAll();
            Debug.Log("[GameUI] Enabled.");
        }

        private void OnDisable()
        {
            if (_playerGlass != null)
                _playerGlass.OnDurabilityChanged -= HandleDurabilityChanged;

            if (_gameManager != null)
            {
                _gameManager.OnDefeatCountChanged -= HandleDefeatCountChanged;
                _gameManager.OnNPCChanged -= HandleNPCChanged;
            }

            if (_npcController != null)
                _npcController.OnCountdownTick -= HandleCountdownTick;

            if (_battleManager != null)
            {
                _battleManager.OnTimingJudged -= HandleTimingJudged;
                _battleManager.OnCheersResolved -= HandleCheersResolved;
            }
        }

        private void Update()
        {
            UpdateTimingGuide();
        }

        // ── イベントハンドラー ──────────────────────────────────────────────

        private void HandleDurabilityChanged(int currentDurability)
        {
            UpdateDurabilityDisplay(currentDurability);
        }

        private void HandleDefeatCountChanged(int defeatCount)
        {
            UpdateDefeatCount(defeatCount);
        }

        private void HandleNPCChanged(NPCData npcData)
        {
            UpdateNPCName(npcData != null ? npcData.NPCName : "");
            ClearCountdown();
        }

        private void HandleCountdownTick(int count)
        {
            if (_countdownText == null) return;
            _countdownText.text = count == 0 ? "乾杯！" : count.ToString();
        }

        private void HandleTimingJudged(TimingGrade grade)
        {
            if (_resultText == null) return;

            _resultText.text = grade switch
            {
                TimingGrade.Perfect => "Perfect!",
                TimingGrade.Great   => "Great!",
                TimingGrade.Good    => "Good!",
                TimingGrade.Miss    => "Miss...",
                _                  => "",
            };
        }

        private void HandleCheersResolved(CheersResult result)
        {
            if (_resultText != null)
            {
                string label = result switch
                {
                    CheersResult.Victory     => "勝利！",
                    CheersResult.Draw        => "引き分け",
                    CheersResult.Defeat      => "敗北...",
                    CheersResult.Whiff       => "スカ！",
                    CheersResult.SelfDestruct => "自爆！！",
                    _                        => "",
                };
                // タイミング表示の後ろに結果を追記
                _resultText.text += $"\n{label}";
            }

            if (_resultClearCoroutine != null)
                StopCoroutine(_resultClearCoroutine);
            _resultClearCoroutine = StartCoroutine(ClearResultAfterDelay());
        }

        // ── タイミングガイドUI ──────────────────────────────────────────────

        private void UpdateTimingGuide()
        {
            if (_timingSystem == null) return;

            bool shouldShow = _timingSystem.IsWindowOpen;

            if (_timingGuidePanel != null && _timingGuideVisible != shouldShow)
            {
                _timingGuidePanel.SetActive(shouldShow);
                _timingGuideVisible = shouldShow;
            }

            if (shouldShow && _timingIndicator != null && _timingBarRect != null)
            {
                float halfWidth = _timingBarRect.rect.width * 0.5f;
                float x = Mathf.Lerp(-halfWidth, halfWidth, _timingSystem.WindowProgress);
                _timingIndicator.anchoredPosition = new Vector2(x, _timingIndicator.anchoredPosition.y);
            }
        }

        // ── 表示更新 ────────────────────────────────────────────────────────

        private void RefreshAll()
        {
            if (_playerGlass != null && _playerGlass.GlassData != null)
            {
                UpdateDurabilityDisplay(_playerGlass.CurrentDurability);
                UpdateGlassName(_playerGlass.GlassData.GlassName);
            }

            UpdateDefeatCount(_gameManager != null ? _gameManager.DefeatCount : 0);
            UpdateNPCName(_gameManager != null && _gameManager.CurrentNPC != null
                ? _gameManager.CurrentNPC.NPCName : "");

            ClearCountdown();

            if (_resultText != null) _resultText.text = "";
            if (_timingGuidePanel != null)
            {
                _timingGuidePanel.SetActive(false);
                _timingGuideVisible = false;
            }
        }

        private void UpdateDurabilityDisplay(int currentDurability)
        {
            if (_playerGlass == null || _playerGlass.GlassData == null) return;

            int max = _playerGlass.GlassData.MaxDurability;

            if (_durabilitySlider != null)
            {
                _durabilitySlider.maxValue = max;
                _durabilitySlider.value = currentDurability;
            }

            if (_durabilityText != null)
                _durabilityText.text = $"{currentDurability} / {max}";
        }

        private void UpdateDefeatCount(int defeatCount)
        {
            if (_defeatCountText != null)
                _defeatCountText.text = $"撃破: {defeatCount}";
        }

        private void UpdateGlassName(string glassName)
        {
            if (_glassNameText != null)
                _glassNameText.text = glassName;
        }

        private void UpdateNPCName(string npcName)
        {
            if (_npcNameText != null)
                _npcNameText.text = npcName;
        }

        private void ClearCountdown()
        {
            if (_countdownText != null)
                _countdownText.text = "";
        }

        private IEnumerator ClearResultAfterDelay()
        {
            yield return new WaitForSeconds(_resultDisplayDuration);
            if (_resultText != null)
                _resultText.text = "";
            _resultClearCoroutine = null;
        }
    }
}
