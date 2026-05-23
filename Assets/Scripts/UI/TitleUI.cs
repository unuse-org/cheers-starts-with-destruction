using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CheersGame.Game;

namespace CheersGame.UI
{
    /// <summary>
    /// タイトル画面のUI制御。
    /// 開始用の乾杯タイミングループと、ゲーム開始時のスケールパルスを担当する。
    /// </summary>
    public class TitleUI : MonoBehaviour
    {
        [Header("Image Elements")]
        [SerializeField] private Image _startImage;

        [Header("Timing Guide")]
        [SerializeField] private TimingSystem _timingSystem;
        [SerializeField] private Sprite _glassSprite;
        [SerializeField] private float _titleWindowDuration = 1.4f;
        [SerializeField] private float _loopRestartDelay = 0.25f;
        [SerializeField] private float _glassStartOffset = 500f;
        [SerializeField] private Vector2 _guidePosition = new Vector2(0f, -900f);
        [SerializeField] private Vector2 _glassSize = new Vector2(320f, 320f);

        [Header("Judge Sprites")]
        [SerializeField] private Image _judgeImage;
        [SerializeField] private Sprite _judgeIncredible;
        [SerializeField] private Sprite _judgePerfect;
        [SerializeField] private Sprite _judgeGreat;
        [SerializeField] private Sprite _judgeGood;
        [SerializeField] private Sprite _judgeBad;

        [Header("Judge Thresholds")]
        [SerializeField] private float _thresholdIncredible = 0.8f;
        [SerializeField] private float _thresholdPerfect = 0.6f;
        [SerializeField] private float _thresholdGreat = 0.4f;
        [SerializeField] private float _thresholdGood = 0.2f;

        [Header("Judge Animation")]
        [SerializeField] private Vector2 _judgePosition = new Vector2(0f, 110f);
        [SerializeField] private Vector2 _judgeSize = new Vector2(900f, 220f);
        [SerializeField] private float _judgeDisplayDuration = 0.9f;
        [SerializeField] private float _startJudgeDisplayDuration = 2.8f;

        private static readonly float PulseDuration = 0.55f;

        private GameObject _timingGuidePanel;
        private RectTransform _leftGlassRect;
        private RectTransform _rightGlassRect;
        private Coroutine _timingLoopCoroutine;
        private Coroutine _judgeCoroutine;
        private bool _isStartAccepted;
        private bool _timingGuideVisible;
        private Vector2 _judgeOrigPos;
        private Vector3 _judgeOrigScale = Vector3.one;
        private Vector3 _startImageOriginalScale = Vector3.one;

        private void Start()
        {
            Debug.Log("[TitleUI] Initialized.");
        }

        private void OnEnable()
        {
            BuildGeneratedUI();
            ResetTitleState();
            StartTimingLoop();
        }

        private void OnDisable()
        {
            StopTimingLoop();

            if (_judgeCoroutine != null)
            {
                StopCoroutine(_judgeCoroutine);
                _judgeCoroutine = null;
            }
        }

        private void Update()
        {
            UpdateTimingGuide();
        }

        /// <summary>
        /// タイトル中の乾杯入力を判定する。
        /// judge_1incredible 相当のタイミングだけゲーム開始を許可する。
        /// </summary>
        public bool TryJudgeStart()
        {
            if (_isStartAccepted) return true;

            float timingScore = _timingSystem != null && _timingSystem.IsWindowOpen
                ? _timingSystem.GetTimingScore()
                : 0f;

            if (_timingSystem != null)
                _timingSystem.CloseWindow();

            Sprite judgeSprite = GetJudgeSprite(timingScore);

            bool isIncredible = timingScore >= _thresholdIncredible;
            PlayJudge(judgeSprite, isIncredible ? _startJudgeDisplayDuration : _judgeDisplayDuration);

            if (!isIncredible)
            {
                Debug.Log($"[TitleUI] Start denied. timing={timingScore:F2}");
                return false;
            }

            Debug.Log($"[TitleUI] Start accepted. timing={timingScore:F2}");
            _isStartAccepted = true;
            StopTimingLoop();
            return true;
        }

        /// <summary>
        /// ゲーム開始時に呼ぶ。スケールパルスを再生して返る。
        /// </summary>
        public IEnumerator PlayStartAnimation()
        {
            if (_startImage == null) yield break;

            float elapsed = 0f;

            while (elapsed < PulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / PulseDuration);
                float scale = 1f + 0.3f * Mathf.Sin(t * Mathf.PI);
                _startImage.transform.localScale = _startImageOriginalScale * scale;
                yield return null;
            }

            _startImage.transform.localScale = _startImageOriginalScale;
        }

        private void BuildGeneratedUI()
        {
            if (_timingSystem == null)
                _timingSystem = FindObjectOfType<TimingSystem>();

            if (_startImage != null)
                _startImageOriginalScale = _startImage.transform.localScale;

            if (_timingGuidePanel == null)
            {
                _timingGuidePanel = new GameObject("TitleTimingGuide", typeof(RectTransform));
                _timingGuidePanel.layer = gameObject.layer;
                _timingGuidePanel.transform.SetParent(transform, false);

                RectTransform panelRect = _timingGuidePanel.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = _guidePosition;
                panelRect.sizeDelta = new Vector2(1080f, 520f);
            }

            if (_leftGlassRect == null)
                _leftGlassRect = CreateGlassImage("LeftGlass", true);

            if (_rightGlassRect == null)
                _rightGlassRect = CreateGlassImage("RightGlass", false);

            if (_judgeImage == null)
            {
                _judgeImage = CreateImage("TitleJudgeImage", transform, _judgeSize);
                _judgeImage.preserveAspect = true;
                _judgeImage.raycastTarget = false;
                _judgeImage.rectTransform.anchoredPosition = _judgePosition;
            }

            _judgeOrigPos = _judgeImage.rectTransform.anchoredPosition;
            _judgeOrigScale = _judgeImage.rectTransform.localScale;
            _judgeImage.gameObject.SetActive(false);
        }

        private RectTransform CreateGlassImage(string objectName, bool flipX)
        {
            Image image = CreateImage(objectName, _timingGuidePanel.transform, _glassSize);
            image.sprite = _glassSprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            RectTransform rect = image.rectTransform;
            rect.localScale = new Vector3(flipX ? -1f : 1f, 1f, 1f);
            return rect;
        }

        private static Image CreateImage(string objectName, Transform parent, Vector2 size)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;

            return go.GetComponent<Image>();
        }

        private void ResetTitleState()
        {
            _isStartAccepted = false;
            _timingGuideVisible = false;

            if (_startImage != null)
                _startImage.transform.localScale = _startImageOriginalScale;

            if (_judgeImage != null)
            {
                _judgeImage.gameObject.SetActive(false);
                ApplyJudge(1f, _judgeOrigPos, 1f);
            }
        }

        private void StartTimingLoop()
        {
            if (_timingLoopCoroutine != null)
                StopCoroutine(_timingLoopCoroutine);

            _timingLoopCoroutine = StartCoroutine(TimingLoopRoutine());
        }

        private void StopTimingLoop()
        {
            if (_timingLoopCoroutine != null)
            {
                StopCoroutine(_timingLoopCoroutine);
                _timingLoopCoroutine = null;
            }

            if (_timingSystem != null)
                _timingSystem.CloseWindow();

            SetTimingGuideVisible(false);
        }

        private IEnumerator TimingLoopRoutine()
        {
            while (isActiveAndEnabled && !_isStartAccepted)
            {
                if (_timingSystem == null)
                    yield break;

                _timingSystem.StartWindow(_titleWindowDuration);
                SetTimingGuideVisible(true);

                while (isActiveAndEnabled && !_isStartAccepted && _timingSystem.IsWindowOpen)
                    yield return null;

                SetTimingGuideVisible(false);

                if (!_isStartAccepted && _loopRestartDelay > 0f)
                    yield return new WaitForSeconds(_loopRestartDelay);
            }

            _timingLoopCoroutine = null;
        }

        private void UpdateTimingGuide()
        {
            if (_timingSystem == null || _leftGlassRect == null || _rightGlassRect == null)
                return;

            bool shouldShow = isActiveAndEnabled && !_isStartAccepted && _timingSystem.IsWindowOpen;
            SetTimingGuideVisible(shouldShow);

            if (!shouldShow) return;

            float x = Mathf.Lerp(_glassStartOffset, 0f, _timingSystem.GlassProgress);
            _leftGlassRect.anchoredPosition = new Vector2(-x, _leftGlassRect.anchoredPosition.y);
            _rightGlassRect.anchoredPosition = new Vector2(x, _rightGlassRect.anchoredPosition.y);
        }

        private void SetTimingGuideVisible(bool visible)
        {
            if (_timingGuidePanel == null || _timingGuideVisible == visible)
                return;

            _timingGuidePanel.SetActive(visible);
            _timingGuideVisible = visible;
        }

        private void PlayJudge(Sprite judgeSprite, float displayDuration)
        {
            if (judgeSprite == null || _judgeImage == null)
                return;

            if (_judgeCoroutine != null)
                StopCoroutine(_judgeCoroutine);

            _judgeCoroutine = StartCoroutine(ShowJudgeRoutine(judgeSprite, displayDuration));
        }

        private IEnumerator ShowJudgeRoutine(Sprite judgeSprite, float displayDuration)
        {
            _judgeImage.sprite = judgeSprite;
            _judgeImage.gameObject.SetActive(true);

            float elapsed = 0f;
            const float InDuration = 0.13f;
            Vector2 startPos = new Vector2(_judgeOrigPos.x, _judgeOrigPos.y + 180f);

            while (elapsed < InDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / InDuration);
                float e = EaseOutQuart(t);
                ApplyJudge(Mathf.Lerp(0.2f, 1.25f, e), Vector2.Lerp(startPos, _judgeOrigPos, e), t);
                yield return null;
            }

            elapsed = 0f;
            const float SettleDuration = 0.25f;
            while (elapsed < SettleDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed;
                float scale = 1f + 0.25f * Mathf.Exp(-10f * t) * Mathf.Cos(22f * t);
                ApplyJudge(scale, _judgeOrigPos, 1f);
                yield return null;
            }

            ApplyJudge(1f, _judgeOrigPos, 1f);

            yield return new WaitForSeconds(displayDuration);

            elapsed = 0f;
            const float OutDuration = 0.15f;
            while (elapsed < OutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / OutDuration);
                float e = EaseInQuart(t);
                ApplyJudge(Mathf.Lerp(1f, 0f, e), _judgeOrigPos, 1f - t);
                yield return null;
            }

            _judgeImage.gameObject.SetActive(false);
            ApplyJudge(1f, _judgeOrigPos, 1f);
            _judgeCoroutine = null;
        }

        private void ApplyJudge(float scale, Vector2 pos, float alpha)
        {
            if (_judgeImage == null) return;

            _judgeImage.rectTransform.localScale = _judgeOrigScale * scale;
            _judgeImage.rectTransform.anchoredPosition = pos;

            Color c = _judgeImage.color;
            c.a = Mathf.Clamp01(alpha);
            _judgeImage.color = c;
        }

        private Sprite GetJudgeSprite(float timingScore)
        {
            if (timingScore >= _thresholdIncredible) return _judgeIncredible;
            if (timingScore >= _thresholdPerfect) return _judgePerfect;
            if (timingScore >= _thresholdGreat) return _judgeGreat;
            if (timingScore >= _thresholdGood) return _judgeGood;
            return _judgeBad;
        }

        private static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
        private static float EaseInQuart(float t) => t * t * t * t;
    }
}
