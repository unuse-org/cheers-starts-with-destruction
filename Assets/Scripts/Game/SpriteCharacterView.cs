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
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public void Show(NPCData data)
        {
            if (_spriteRenderer != null)
                _spriteRenderer.sprite = data.FaceSprite;

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
