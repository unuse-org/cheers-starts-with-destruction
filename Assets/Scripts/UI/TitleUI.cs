using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CheersGame.UI
{
    /// <summary>
    /// タイトル画面のUI制御。
    /// start.png の表示と、ゲーム開始時のスケールパルスアニメーションを担当する。
    /// </summary>
    public class TitleUI : MonoBehaviour
    {
        [Header("Image Elements")]
        [SerializeField] private Image _startImage;

        private static readonly float PulseDuration = 0.55f;

        private void Start()
        {
            Debug.Log("[TitleUI] Initialized.");
        }

        /// <summary>
        /// ゲーム開始時に呼ぶ。スケールパルスを再生して返る。
        /// </summary>
        public IEnumerator PlayStartAnimation()
        {
            if (_startImage == null) yield break;

            Vector3 originalScale = _startImage.transform.localScale;
            float elapsed = 0f;

            while (elapsed < PulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / PulseDuration);
                float scale = 1f + 0.3f * Mathf.Sin(t * Mathf.PI);
                _startImage.transform.localScale = originalScale * scale;
                yield return null;
            }

            _startImage.transform.localScale = originalScale;
        }
    }
}
