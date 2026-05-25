using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CheersGame.Game;

namespace CheersGame.UI
{
    /// <summary>
    /// リザルト画面のUI制御。
    /// 結果画像をレシートのように上から展開し、撃破数を表示する。
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager _gameManager;

        [Header("Result Image")]
        [SerializeField] private Image _resultImage;
        [SerializeField] private Sprite _spriteExcellent;
        [SerializeField] private Sprite _spriteGood;
        [SerializeField] private Sprite _spriteBad;

        [Header("Thresholds")]
        [Tooltip("この撃破数以上で Excellent")]
        [SerializeField] private int _excellentThreshold = 5;
        [Tooltip("この撃破数以上で Good（未満は Bad）")]
        [SerializeField] private int _goodThreshold = 2;

        [Header("Text Elements")]
        [Tooltip("撃破数を画像で表示（結果画像の上に重ねる）")]
        [SerializeField] private ImageNumberDisplay _scoreNumberDisplay;
        [SerializeField] private TextMeshProUGUI _instructionText;

        [Header("Animation")]
        [Tooltip("レシートの展開にかかる秒数")]
        [SerializeField] private float _rollDuration = 1.2f;

        private Coroutine _showCoroutine;

        private void OnEnable()
        {
            if (_showCoroutine != null)
                StopCoroutine(_showCoroutine);
            _showCoroutine = StartCoroutine(ShowResult());
            Debug.Log("[ScoreUI] Enabled.");
        }

        private void OnDisable()
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
                _showCoroutine = null;
            }
        }

        private IEnumerator ShowResult()
        {
            int count = _gameManager != null ? _gameManager.DefeatCount : 0;

            Sprite sprite = count >= _excellentThreshold ? _spriteExcellent
                          : count >= _goodThreshold      ? _spriteGood
                          :                                _spriteBad;

            // 結果画像をレシート展開用に設定
            if (_resultImage != null && sprite != null)
            {
                _resultImage.sprite     = sprite;
                _resultImage.type       = Image.Type.Filled;
                _resultImage.fillMethod = Image.FillMethod.Vertical;
                _resultImage.fillOrigin = (int)Image.OriginVertical.Top;
                _resultImage.fillAmount = 0f;
                _resultImage.gameObject.SetActive(true);
            }

            // 数字表示は最初非表示
            if (_scoreNumberDisplay != null)
            {
                _scoreNumberDisplay.SetNumber(count);
                _scoreNumberDisplay.gameObject.SetActive(false);
            }
            if (_instructionText != null)
            {
                _instructionText.text = "乾杯してタイトルへ";
                SetAlpha(_instructionText, 0f);
                _instructionText.gameObject.SetActive(false);
            }

            // Phase 1: レシートロールアウト (0 → 1)
            float elapsed = 0f;
            while (elapsed < _rollDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _rollDuration);
                if (_resultImage != null)
                    _resultImage.fillAmount = Mathf.SmoothStep(0f, 1f, t);
                yield return null;
            }
            if (_resultImage != null) _resultImage.fillAmount = 1f;

            yield return new WaitForSeconds(0.15f);

            // Phase 2: 撃破数ポップイン（バネ振動）
            if (_scoreNumberDisplay != null)
            {
                _scoreNumberDisplay.gameObject.SetActive(true);
                yield return SpringPopIn(_scoreNumberDisplay.transform as RectTransform);
            }

            yield return new WaitForSeconds(0.3f);

            // Phase 3: 案内テキストフェードイン
            if (_instructionText != null)
            {
                _instructionText.gameObject.SetActive(true);
                yield return FadeIn(_instructionText, 0.5f);
            }

            _showCoroutine = null;
        }

        private IEnumerator SpringPopIn(RectTransform rt)
        {
            float duration = 0.45f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed;
                float scale = 1f + 0.55f * Mathf.Exp(-10f * t) * Mathf.Cos(26f * t);
                rt.localScale = Vector3.one * Mathf.Max(0f, scale);
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        private IEnumerator FadeIn(TextMeshProUGUI tmp, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(tmp, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            SetAlpha(tmp, 1f);
        }

        private static void SetAlpha(TextMeshProUGUI tmp, float alpha)
        {
            Color c = tmp.color;
            c.a = alpha;
            tmp.color = c;
        }
    }
}
