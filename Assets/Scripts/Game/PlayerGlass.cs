using System;
using UnityEngine;
using CheersGame.Data;

namespace CheersGame.Game
{
    /// <summary>
    /// プレイヤーのグラス状態を管理する。
    /// 耐久値の増減とグラス破壊イベントを担当。
    /// </summary>
    public class PlayerGlass : MonoBehaviour
    {
        public GlassData GlassData { get; private set; }
        public int CurrentDurability { get; private set; }
        public bool IsBroken => CurrentDurability <= 0;

        /// <summary>耐久値が変化したときに発火（現在の耐久値を通知）</summary>
        public event Action<int> OnDurabilityChanged;

        /// <summary>グラスが破壊されたときに発火</summary>
        public event Action OnGlassBroken;

        /// <summary>
        /// グラスデータで初期化する。ゲーム開始時に呼ぶ。
        /// </summary>
        public void Initialize(GlassData data)
        {
            GlassData = data;
            CurrentDurability = data.MaxDurability;
            OnDurabilityChanged?.Invoke(CurrentDurability);
        }

        /// <summary>
        /// ダメージを受ける。GlassDataのDamageMultiplierが適用される。
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (IsBroken) return;

            int actualDamage = Mathf.RoundToInt(damage * GlassData.DamageMultiplier);
            CurrentDurability = Mathf.Max(0, CurrentDurability - actualDamage);

            Debug.Log($"[PlayerGlass] Damage={actualDamage}, Durability={CurrentDurability}/{GlassData.MaxDurability}");
            OnDurabilityChanged?.Invoke(CurrentDurability);

            if (IsBroken)
            {
                Debug.Log("[PlayerGlass] Glass broken!");
                OnGlassBroken?.Invoke();
            }
        }
    }
}
