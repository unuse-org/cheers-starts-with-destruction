using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CheersGame.Game;

namespace CheersGame.UI
{
    /// <summary>
    /// ゲーム画面のUI制御。
    /// 耐久値バー・撃破数・グラス名をリアルタイム表示する。
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private PlayerGlass _playerGlass;

        [Header("Durability")]
        [SerializeField] private Slider _durabilitySlider;
        [SerializeField] private TextMeshProUGUI _durabilityText;

        [Header("Game Info")]
        [SerializeField] private TextMeshProUGUI _defeatCountText;
        [SerializeField] private TextMeshProUGUI _glassNameText;

        private void OnEnable()
        {
            if (_playerGlass != null)
            {
                _playerGlass.OnDurabilityChanged += HandleDurabilityChanged;
            }

            if (_gameManager != null)
            {
                _gameManager.OnDefeatCountChanged += HandleDefeatCountChanged;
            }

            RefreshAll();
            Debug.Log("[GameUI] Enabled.");
        }

        private void OnDisable()
        {
            if (_playerGlass != null)
            {
                _playerGlass.OnDurabilityChanged -= HandleDurabilityChanged;
            }

            if (_gameManager != null)
            {
                _gameManager.OnDefeatCountChanged -= HandleDefeatCountChanged;
            }
        }

        private void HandleDurabilityChanged(int currentDurability)
        {
            UpdateDurabilityDisplay(currentDurability);
        }

        private void HandleDefeatCountChanged(int defeatCount)
        {
            UpdateDefeatCount(defeatCount);
        }

        private void RefreshAll()
        {
            if (_playerGlass != null && _playerGlass.GlassData != null)
            {
                UpdateDurabilityDisplay(_playerGlass.CurrentDurability);
                UpdateGlassName(_playerGlass.GlassData.GlassName);
            }

            UpdateDefeatCount(_gameManager != null ? _gameManager.DefeatCount : 0);
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
            {
                _durabilityText.text = $"{currentDurability} / {max}";
            }
        }

        private void UpdateDefeatCount(int defeatCount)
        {
            if (_defeatCountText != null)
            {
                _defeatCountText.text = $"撃破: {defeatCount}";
            }
        }

        private void UpdateGlassName(string glassName)
        {
            if (_glassNameText != null)
            {
                _glassNameText.text = glassName;
            }
        }
    }
}
