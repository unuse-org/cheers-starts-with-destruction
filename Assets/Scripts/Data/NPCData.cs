using UnityEngine;

namespace CheersGame.Data
{
    /// <summary>
    /// NPCの種類を定義するScriptableObject。
    /// Assets/Data/ にアセットを作成して使用する。
    /// </summary>
    [CreateAssetMenu(fileName = "NPCData", menuName = "CheersGame/NPCData")]
    public class NPCData : ScriptableObject
    {
        [Header("Basic Info")]
        public string NPCName;
        public Sprite FaceSprite;
        public GameObject ModelPrefab;

        [Header("Battle Parameters")]
        [Tooltip("反応速度（将来拡張用）")]
        public float ReactionSpeed = 1.0f;

        [Header("Animation States")]
        public string AnimStateCheers;
        public string AnimStateWin;
        public string AnimStateLose;
        public string AnimStateLoseLoop;
    }
}
