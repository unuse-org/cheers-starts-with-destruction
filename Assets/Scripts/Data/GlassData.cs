using UnityEngine;

namespace CheersGame.Data
{
    /// <summary>
    /// グラスの種類を定義するScriptableObject。
    /// Assets/Data/ にアセットを作成して使用する。
    /// </summary>
    [CreateAssetMenu(fileName = "GlassData", menuName = "CheersGame/GlassData")]
    public class GlassData : ScriptableObject
    {
        [Header("Basic Info")]
        public string GlassName;
        public int MaxDurability = 100;
        public Sprite Icon;
        public GameObject ModelPrefab;

        [Header("Battle Parameters")]
        [Tooltip("被ダメージ倍率（1.0 = 通常）")]
        public float DamageMultiplier = 1.0f;
    }
}
