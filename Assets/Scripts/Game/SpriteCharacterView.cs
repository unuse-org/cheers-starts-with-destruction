using UnityEngine;
using CheersGame.Data;

namespace CheersGame.Game
{
    /// <summary>
    /// PNG スプライトでキャラクターを表示する ICharacterView 実装。
    /// Live2D 移行時はこのクラスを差し替えるだけでよい。
    /// </summary>
    public class SpriteCharacterView : MonoBehaviour, ICharacterView
    {
        private GameObject _currentModel;

        public void Show(NPCData data)
        {
            // 前のモデル削除
            if (_currentModel != null)
            {
                Destroy(_currentModel);
                _currentModel = null;
            }

            // Live2Dモデル生成
            if (data.ModelPrefab != null)
            {
                _currentModel = Instantiate(data.ModelPrefab, transform);
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
