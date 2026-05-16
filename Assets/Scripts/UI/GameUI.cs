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
        [SerializeField] private CompoundNumberDisplay _durabilityDisplay;

        [Header("Game Info")]
        [SerializeField] private CompoundNumberDisplay _defeatCountDisplay;
        [SerializeField] private TextMeshProUGUI _glassNameText;

        [Header("NPC Info")]
        [SerializeField] private TextMeshProUGUI _npcNameText;

        [Header("Countdown")]
        [SerializeField] private ImageNumberDisplay _countdownDisplay;

        [Header("Timing Guide")]
        [Tooltip("タイミングガイド全体のパネル。ウィンドウ開閉に合わせて表示/非表示する。")]
        [SerializeField] private GameObject _timingGuidePanel;
        [Tooltip("左から移動するジョッキ画像の RectTransform")]
        [SerializeField] private RectTransform _leftGlassRect;
        [Tooltip("右から移動するジョッキ画像の RectTransform")]
        [SerializeField] private RectTransform _rightGlassRect;
        [Tooltip("ジョッキの初期 X 位置（中央からのオフセット px）")]
        [SerializeField] private float _glassStartOffset = 500f;

        [Header("Milestone Banner")]
        [Tooltip("score_upper.png を表示する Image（右上に配置）")]
        [SerializeField] private Image _milestoneImage;
        [Tooltip("撃破数を表示する画像ベース数字表示")]
        [SerializeField] private ImageNumberDisplay _milestoneDisplay;
        [Tooltip("バナーを表示し続ける秒数")]
        [SerializeField] private float _milestoneDuration = 2f;
        [Tooltip("5 の倍数ごとに発火（変更可能）")]
        [SerializeField] private int _milestoneInterval = 5;

        [Header("Game Over Overlay")]
        [Tooltip("HP0時にGameScreen上に表示する全画面オーバーレイ (CanvasGroup)")]
        [SerializeField] private CanvasGroup _gameOverOverlay;
        [Tooltip("オーバーレイのフェードイン秒数")]
        [SerializeField] private float _gameOverFadeInDuration = 0.3f;
        [Tooltip("オーバーレイを表示し続ける秒数")]
        [SerializeField] private float _gameOverHoldDuration = 1.5f;

        [Header("Result")]
        [SerializeField] private Image _resultImage;
        [SerializeField] private Image _scoreImage;
        [Tooltip("score.png の左に並べる撃破数（画像ベース数字表示）")]
        [SerializeField] private ImageNumberDisplay _scoreNumberDisplay;
        [Tooltip("結果画像を表示する秒数")]
        [SerializeField] private float _resultDisplayDuration = 1.5f;

        [Header("Judge Sprites")]
        [Tooltip("timingScore >= threshold で使用 (0.8)")]
        [SerializeField] private Sprite _judgeIncredible;
        [SerializeField] private Sprite _judgePerfect;
        [SerializeField] private Sprite _judgeGreat;
        [SerializeField] private Sprite _judgeGood;
        [Tooltip("それ以外")]
        [SerializeField] private Sprite _judgeBad;

        [Header("Judge Thresholds")]
        [SerializeField] private float _thresholdIncredible = 0.8f;
        [SerializeField] private float _thresholdPerfect    = 0.6f;
        [SerializeField] private float _thresholdGreat      = 0.4f;
        [SerializeField] private float _thresholdGood       = 0.2f;

        private Coroutine _resultCoroutine;
        private bool _timingGuideVisible;
        private Vector2 _resultOrigPos;
        private Vector2 _scoreOrigPos;
        private float _lastTimingScore;
        private Vector2 _milestoneOrigPos;
        private Coroutine _milestoneCoroutine;

        private void Start()
        {
            if (_resultImage   != null) _resultOrigPos   = _resultImage.rectTransform.anchoredPosition;
            if (_scoreImage    != null) _scoreOrigPos    = _scoreImage.rectTransform.anchoredPosition;
            if (_milestoneImage != null) _milestoneOrigPos = _milestoneImage.rectTransform.anchoredPosition;
        }

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
                _battleManager.OnTimingJudged  += HandleTimingJudged;
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
                _battleManager.OnTimingJudged  -= HandleTimingJudged;
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

            if (defeatCount > 0 && defeatCount % _milestoneInterval == 0)
            {
                if (_milestoneCoroutine != null) StopCoroutine(_milestoneCoroutine);
                _milestoneCoroutine = StartCoroutine(ShowMilestoneAnimation(defeatCount));
            }
        }

        private void HandleNPCChanged(NPCData npcData)
        {
            UpdateNPCName(npcData != null ? npcData.NPCName : "");
            ClearCountdown();
        }

        private void HandleCountdownTick(int count)
        {
            if (_countdownDisplay == null) return;

            if (count == 0)
            {
                _countdownDisplay.gameObject.SetActive(false);
            }
            else
            {
                _countdownDisplay.gameObject.SetActive(true);
                _countdownDisplay.SetNumber(count);
            }
        }

        private void HandleTimingJudged(float score) => _lastTimingScore = score;

        private void HandleCheersResolved(CheersResult _)
        {
            Sprite sprite = GetJudgeSprite(_lastTimingScore);

            if (sprite == null) return;

            if (_resultCoroutine != null)
                StopCoroutine(_resultCoroutine);
            _resultCoroutine = StartCoroutine(ShowResultAnimation(sprite));
        }

        // ── マイルストーンバナー ────────────────────────────────────────────

        private IEnumerator ShowMilestoneAnimation(int count)
        {
            if (_milestoneImage == null) yield break;

            if (_milestoneDisplay != null)
            {
                _milestoneDisplay.SetNumber(count);
                _milestoneDisplay.gameObject.SetActive(true);
                _milestoneDisplay.SetAlpha(0f);
            }

            _milestoneImage.gameObject.SetActive(true);

            // Phase 1: 右外から EaseOutBack でスライドイン (0.35s)
            const float SlideDuration = 0.35f;
            Vector2 offscreen = new Vector2(_milestoneOrigPos.x + 500f, _milestoneOrigPos.y);
            float elapsed = 0f;

            while (elapsed < SlideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / SlideDuration);
                float e = EaseOutBack(t);
                _milestoneImage.rectTransform.anchoredPosition = Vector2.Lerp(offscreen, _milestoneOrigPos, e);
                if (_milestoneDisplay != null) _milestoneDisplay.transform.position = _milestoneImage.transform.position;
                yield return null;
            }
            _milestoneImage.rectTransform.anchoredPosition = _milestoneOrigPos;

            // Phase 2: 数字フェードイン (0.2s)
            if (_milestoneDisplay != null)
            {
                elapsed = 0f;
                const float FadeDuration = 0.2f;
                while (elapsed < FadeDuration)
                {
                    elapsed += Time.deltaTime;
                    _milestoneDisplay.SetAlpha(Mathf.Clamp01(elapsed / FadeDuration));
                    yield return null;
                }
                _milestoneDisplay.SetAlpha(1f);
            }

            // Phase 3: ホールド
            yield return new WaitForSeconds(_milestoneDuration);

            // Phase 4: 右へスライドアウト (0.25s)
            elapsed = 0f;
            const float ExitDuration = 0.25f;
            Vector2 startPos = _milestoneOrigPos;

            while (elapsed < ExitDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / ExitDuration);
                float e = EaseInQuart(t);
                Vector2 pos = Vector2.Lerp(startPos, offscreen, e);
                _milestoneImage.rectTransform.anchoredPosition = pos;
                if (_milestoneDisplay != null) _milestoneDisplay.transform.position = _milestoneImage.transform.position;
                yield return null;
            }

            _milestoneImage.gameObject.SetActive(false);
            if (_milestoneDisplay != null) _milestoneDisplay.gameObject.SetActive(false);
            _milestoneCoroutine = null;
        }

        private static void SetTextAlpha(TextMeshProUGUI tmp, float alpha)
        {
            Color c = tmp.color;
            c.a = alpha;
            tmp.color = c;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        // ── ゲームオーバーオーバーレイ ──────────────────────────────────────

        /// <summary>
        /// HP0時にGameScreen上で呼ぶ。オーバーレイをフェードイン→ホールドして返る。
        /// 返った後にGameManagerがScore画面へ遷移する。
        /// </summary>
        public IEnumerator PlayGameOverOverlay()
        {
            if (_gameOverOverlay == null) yield break;

            _gameOverOverlay.alpha = 0f;
            _gameOverOverlay.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < _gameOverFadeInDuration)
            {
                elapsed += Time.deltaTime;
                _gameOverOverlay.alpha = Mathf.Clamp01(elapsed / _gameOverFadeInDuration);
                yield return null;
            }
            _gameOverOverlay.alpha = 1f;

            yield return new WaitForSeconds(_gameOverHoldDuration);
        }

        // ── 結果アニメーション（パチンコ演出）──────────────────────────────

        private IEnumerator ShowResultAnimation(Sprite judgeSprite)
        {
            if (_resultImage == null) yield break;

            // 初期状態セット（上空から落下開始位置）
            _resultImage.sprite = judgeSprite;
            ApplyJudge(0f, new Vector2(_resultOrigPos.x, _resultOrigPos.y + 220f), -15f, 0f);
            ApplyScore(0f, 0f);
            _resultImage.gameObject.SetActive(true);
            if (_scoreImage != null) _scoreImage.gameObject.SetActive(true);
            if (_scoreNumberDisplay != null) _scoreNumberDisplay.gameObject.SetActive(true);

            // AddDefeat() は OnCheersResolved の後に呼ばれるため、1フレーム待って正しい値を読む
            yield return null;

            if (_scoreNumberDisplay != null)
                _scoreNumberDisplay.SetNumber(_gameManager != null ? _gameManager.DefeatCount : 0);

            // Phase 1: SLAM in (0.13s) — 上から高速落下＋回転
            float elapsed = 0f;
            const float SlamDuration = 0.13f;
            Vector2 slamStart = new Vector2(_resultOrigPos.x, _resultOrigPos.y + 220f);

            while (elapsed < SlamDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / SlamDuration);
                float e = EaseOutQuart(t);

                ApplyJudge(
                    Mathf.Lerp(0f, 1.45f, e),
                    Vector2.Lerp(slamStart, _resultOrigPos, e),
                    Mathf.Lerp(-15f, 0f, e),
                    Mathf.Clamp01(t * 6f));

                // score は少し遅れて出現
                float st = Mathf.Clamp01((t - 0.25f) / 0.75f);
                ApplyScore(Mathf.Lerp(0f, 1.35f, EaseOutQuart(st)), Mathf.Clamp01(st * 5f));

                yield return null;
            }

            // Phase 2: Spring settle (0.4s) — 減衰振動でバネっぽく落ち着く
            elapsed = 0f;
            const float SpringDuration = 0.4f;

            while (elapsed < SpringDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed;

                float judgeScale = 1f + 0.45f * Mathf.Exp(-9f  * t) * Mathf.Cos(22f * t);
                float judgeRot   =       -8f * Mathf.Exp(-12f * t) * Mathf.Cos(18f * t);
                float scoreScale = 1f + 0.35f * Mathf.Exp(-10f * t) * Mathf.Cos(20f * t);

                ApplyJudge(judgeScale, _resultOrigPos, judgeRot, 1f);
                ApplyScore(scoreScale, 1f);
                yield return null;
            }

            ApplyJudge(1f, _resultOrigPos, 0f, 1f);
            ApplyScore(1f, 1f);

            // Phase 3: Hold
            yield return new WaitForSeconds(_resultDisplayDuration);

            // Phase 4: Exit — 素早くスケールアウト＋フェード (0.15s)
            elapsed = 0f;
            const float ExitDuration = 0.15f;

            while (elapsed < ExitDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / ExitDuration);
                float e = EaseInQuart(t);
                ApplyJudge(Mathf.Lerp(1f, 0f, e), _resultOrigPos, 0f, 1f - t);
                ApplyScore(Mathf.Lerp(1f, 0f, e), 1f - t);
                yield return null;
            }

            _resultImage.gameObject.SetActive(false);
            if (_scoreImage != null) _scoreImage.gameObject.SetActive(false);
            if (_scoreNumberDisplay != null) _scoreNumberDisplay.gameObject.SetActive(false);
            _resultCoroutine = null;
        }

        private void ApplyJudge(float scale, Vector2 pos, float rotDeg, float alpha)
        {
            if (_resultImage == null) return;
            _resultImage.rectTransform.localScale        = Vector3.one * scale;
            _resultImage.rectTransform.anchoredPosition  = pos;
            _resultImage.rectTransform.localRotation     = Quaternion.Euler(0f, 0f, rotDeg);
            Color c = _resultImage.color;
            c.a = Mathf.Clamp01(alpha);
            _resultImage.color = c;
        }

        private void ApplyScore(float scale, float alpha)
        {
            if (_scoreImage != null)
            {
                _scoreImage.rectTransform.localScale = Vector3.one * scale;
                Color c = _scoreImage.color;
                c.a = Mathf.Clamp01(alpha);
                _scoreImage.color = c;
            }

            if (_scoreNumberDisplay != null)
            {
                _scoreNumberDisplay.transform.localScale = Vector3.one * scale;
                _scoreNumberDisplay.SetAlpha(alpha);
            }
        }

        private Sprite GetJudgeSprite(float timingScore)
        {
            if (timingScore >= _thresholdIncredible) return _judgeIncredible;
            if (timingScore >= _thresholdPerfect)    return _judgePerfect;
            if (timingScore >= _thresholdGreat)      return _judgeGreat;
            if (timingScore >= _thresholdGood)       return _judgeGood;
            return _judgeBad;
        }

        private static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
        private static float EaseInQuart(float t)  => t * t * t * t;

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

            if (!shouldShow) return;

            float x = Mathf.Lerp(_glassStartOffset, 0f, _timingSystem.GlassProgress);
            if (_leftGlassRect  != null) _leftGlassRect.anchoredPosition  = new Vector2(-x, _leftGlassRect.anchoredPosition.y);
            if (_rightGlassRect != null) _rightGlassRect.anchoredPosition = new Vector2( x, _rightGlassRect.anchoredPosition.y);
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

            if (_resultImage       != null) _resultImage.gameObject.SetActive(false);
            if (_scoreImage        != null) _scoreImage.gameObject.SetActive(false);
            if (_scoreNumberDisplay != null) _scoreNumberDisplay.gameObject.SetActive(false);
            if (_gameOverOverlay != null) { _gameOverOverlay.alpha = 0f; _gameOverOverlay.gameObject.SetActive(false); }
            if (_milestoneImage  != null) _milestoneImage.gameObject.SetActive(false);
            if (_milestoneDisplay != null) _milestoneDisplay.gameObject.SetActive(false);
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

            if (_durabilityDisplay != null)
                _durabilityDisplay.SetDurability(currentDurability, max);
        }

        private void UpdateDefeatCount(int defeatCount)
        {
            if (_defeatCountDisplay != null)
                _defeatCountDisplay.SetDefeatCount(defeatCount);
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
            if (_countdownDisplay != null)
                _countdownDisplay.gameObject.SetActive(false);
        }
    }
}
