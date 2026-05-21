using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CheersGame.Data;

namespace CheersGame.UI
{
    /// <summary>
    /// 数字を画像で表示するコンポーネント。
    /// 桁数に応じて動的にImage要素を生成・管理する。
    /// </summary>
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class ImageNumberDisplay : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private NumberDisplaySettings _settings;

        [Header("Prefab")]
        [Tooltip("数字1桁を表示するImageコンポーネント付きGameObject")]
        [SerializeField] private GameObject _digitPrefab;

        private List<Image> _digitPool = new List<Image>();
        private HorizontalLayoutGroup _layoutGroup;
        private int _currentNumber = -1;

        private void Awake()
        {
            _layoutGroup = GetComponent<HorizontalLayoutGroup>();
            if (_layoutGroup != null && _settings != null)
            {
                _layoutGroup.spacing = _settings.spacing;
                _layoutGroup.childAlignment = TextAnchor.MiddleCenter;
                _layoutGroup.childControlWidth = true;
                _layoutGroup.childControlHeight = true;
                _layoutGroup.childForceExpandWidth = false;
                _layoutGroup.childForceExpandHeight = false;
            }
        }

        /// <summary>
        /// 表示する数字を設定する。
        /// </summary>
        public void SetNumber(int number)
        {
            if (_settings == null)
            {
                Debug.LogWarning("[ImageNumberDisplay] Settings is not assigned.");
                return;
            }

            if (number < 0)
            {
                Debug.LogWarning($"[ImageNumberDisplay] Negative numbers are not supported: {number}");
                number = 0;
            }

            // 同じ数字の場合は再描画をスキップ
            if (_currentNumber == number) return;
            _currentNumber = number;

            // 数字を文字列に変換して各桁を取得
            string numStr = number.ToString();
            int digitCount = numStr.Length;

            // 必要な桁数分のImageを確保
            EnsureDigitCount(digitCount);

            // 各桁に対応する画像を設定
            for (int i = 0; i < digitCount; i++)
            {
                int digit = numStr[i] - '0'; // 文字を数値に変換
                Sprite sprite = _settings.GetDigitSprite(digit);

                if (sprite != null && _digitPool[i] != null)
                {
                    _digitPool[i].sprite = sprite;
                    _digitPool[i].gameObject.SetActive(true);
                }
            }

            // 余った桁は非表示
            for (int i = digitCount; i < _digitPool.Count; i++)
            {
                _digitPool[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// すべての桁のアルファ値を設定する。
        /// </summary>
        public void SetAlpha(float alpha)
        {
            alpha = Mathf.Clamp01(alpha);

            foreach (Image digit in _digitPool)
            {
                if (digit != null && digit.gameObject.activeSelf)
                {
                    Color c = digit.color;
                    c.a = alpha;
                    digit.color = c;
                }
            }
        }

        /// <summary>
        /// 指定された桁数分のImageを確保する。
        /// 不足している場合は新しく生成し、余っている場合は非表示にする。
        /// </summary>
        private void EnsureDigitCount(int count)
        {
            // 不足分を生成
            while (_digitPool.Count < count)
            {
                Image newDigit = CreateDigit();
                if (newDigit != null)
                {
                    _digitPool.Add(newDigit);
                }
                else
                {
                    Debug.LogError("[ImageNumberDisplay] Failed to create digit Image.");
                    break;
                }
            }
        }

        /// <summary>
        /// 新しい桁用のImageを生成する。
        /// </summary>
        private Image CreateDigit()
        {
            if (_digitPrefab == null)
            {
                // Prefabが未設定の場合は動的に生成
                GameObject digitObj = new GameObject("Digit", typeof(RectTransform), typeof(Image));
                digitObj.transform.SetParent(transform, false);

                Image img = digitObj.GetComponent<Image>();
                img.preserveAspect = true;
                img.raycastTarget = false;

                return img;
            }
            else
            {
                // Prefabから生成
                GameObject digitObj = Instantiate(_digitPrefab, transform);
                digitObj.name = $"Digit_{_digitPool.Count}";

                Image img = digitObj.GetComponent<Image>();
                if (img == null)
                {
                    Debug.LogError("[ImageNumberDisplay] Digit prefab must have an Image component.");
                    Destroy(digitObj);
                    return null;
                }

                img.preserveAspect = true;
                img.raycastTarget = false;

                return img;
            }
        }
    }
}
