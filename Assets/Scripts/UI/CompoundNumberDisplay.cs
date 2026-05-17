using UnityEngine;
using TMPro;

namespace CheersGame.UI
{
    /// <summary>
    /// テキストと数字画像を組み合わせて表示するコンポーネント。
    /// 「撃破: 10」や「100 / 200」のような複合的な表示に使用する。
    /// </summary>
    public class CompoundNumberDisplay : MonoBehaviour
    {
        [Header("Prefix (Optional)")]
        [SerializeField] private TextMeshProUGUI _prefixText;

        [Header("Number Display")]
        [SerializeField] private ImageNumberDisplay _numberDisplay;

        [Header("Middle Text (Optional)")]
        [SerializeField] private TextMeshProUGUI _middleText;

        [Header("Second Number (Optional)")]
        [SerializeField] private ImageNumberDisplay _secondNumberDisplay;

        /// <summary>
        /// 撃破数を表示する（"撃破: X"形式）。
        /// </summary>
        public void SetDefeatCount(int count)
        {
            if (_prefixText != null)
            {
                _prefixText.text = "撃破: ";
                _prefixText.gameObject.SetActive(true);
            }

            if (_numberDisplay != null)
            {
                _numberDisplay.SetNumber(count);
                _numberDisplay.gameObject.SetActive(true);
            }

            if (_middleText != null)
                _middleText.gameObject.SetActive(false);

            if (_secondNumberDisplay != null)
                _secondNumberDisplay.gameObject.SetActive(false);
        }

        /// <summary>
        /// 耐久値を表示する（"X / Y"形式）。
        /// </summary>
        public void SetDurability(int current, int max)
        {
            if (_prefixText != null)
                _prefixText.gameObject.SetActive(false);

            if (_numberDisplay != null)
            {
                _numberDisplay.SetNumber(current);
                _numberDisplay.gameObject.SetActive(true);
            }

            if (_middleText != null)
            {
                _middleText.text = " / ";
                _middleText.gameObject.SetActive(true);
            }

            if (_secondNumberDisplay != null)
            {
                _secondNumberDisplay.SetNumber(max);
                _secondNumberDisplay.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// すべての要素のアルファ値を設定する。
        /// </summary>
        public void SetAlpha(float alpha)
        {
            alpha = Mathf.Clamp01(alpha);

            if (_prefixText != null)
            {
                Color c = _prefixText.color;
                c.a = alpha;
                _prefixText.color = c;
            }

            if (_numberDisplay != null)
                _numberDisplay.SetAlpha(alpha);

            if (_middleText != null)
            {
                Color c = _middleText.color;
                c.a = alpha;
                _middleText.color = c;
            }

            if (_secondNumberDisplay != null)
                _secondNumberDisplay.SetAlpha(alpha);
        }

    }
}
