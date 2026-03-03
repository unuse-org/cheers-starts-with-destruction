using UnityEngine;
using TMPro;
using CheersGame.Game;

namespace CheersGame.UI
{
    /// <summary>
    /// スコア画面のUI制御。
    /// 最終スコアとタイトルへの案内を表示する。
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager _gameManager;

        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _instructionText;

        private void OnEnable()
        {
            if (_gameManager != null && _scoreText != null)
            {
                _scoreText.text = $"{_gameManager.DefeatCount}人抜き達成！";
            }

            if (_instructionText != null)
            {
                _instructionText.text = "ジョッキを振ってタイトルへ";
            }

            Debug.Log("[ScoreUI] Enabled.");
        }
    }
}
