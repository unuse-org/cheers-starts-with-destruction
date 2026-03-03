using UnityEngine;
using TMPro;

namespace CheersGame.UI
{
    /// <summary>
    /// タイトル画面のUI制御。
    /// ゲームタイトルと操作案内のテキストを表示する。
    /// </summary>
    public class TitleUI : MonoBehaviour
    {
        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _instructionText;

        private void Start()
        {
            if (_titleText != null)
            {
                _titleText.text = "乾杯の音頭は破壊から";
            }

            if (_instructionText != null)
            {
                _instructionText.text = "ジョッキを振ってスタート！";
            }

            Debug.Log("[TitleUI] Initialized.");
        }
    }
}
