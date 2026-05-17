using UnityEngine;

namespace CheersGame.Data
{
    /// <summary>
    /// 数字画像表示の設定を保持するScriptableObject。
    /// 0-9の数字Spriteとレイアウト設定を管理する。
    /// </summary>
    [CreateAssetMenu(fileName = "NumberDisplaySettings", menuName = "CheersGame/NumberDisplaySettings")]
    public class NumberDisplaySettings : ScriptableObject
    {
        [Header("Digit Sprites")]
        [Tooltip("0-9の数字画像を順番に設定（インデックス0が'0'、インデックス9が'9'）")]
        public Sprite[] digitSprites = new Sprite[10];

        [Header("Layout")]
        [Tooltip("桁間のスペース（ピクセル）")]
        public float spacing = 10f;

        [Tooltip("デフォルトスケール")]
        public float defaultScale = 1f;

        /// <summary>
        /// 指定した数字（0-9）に対応するSpriteを取得する。
        /// </summary>
        public Sprite GetDigitSprite(int digit)
        {
            if (digit < 0 || digit > 9)
            {
                Debug.LogWarning($"[NumberDisplaySettings] Invalid digit: {digit}. Must be 0-9.");
                return null;
            }

            if (digitSprites == null || digitSprites.Length != 10)
            {
                Debug.LogError("[NumberDisplaySettings] digitSprites array is not properly initialized.");
                return null;
            }

            return digitSprites[digit];
        }
    }
}
