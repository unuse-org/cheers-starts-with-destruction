using System;

namespace CheersGame.Input
{
    /// <summary>
    /// センサー入力の抽象インターフェース。
    /// モック実装とセンサー実装の両方がこのインターフェースを実装する。
    /// </summary>
    public interface ISensorInput
    {
        /// <summary>
        /// 乾杯アクション（ジョッキを振る動作）が検出されたときに発火
        /// </summary>
        event Action<CheersInputData> OnCheersDetected;
    }

    /// <summary>
    /// 乾杯アクションの入力データ
    /// </summary>
    public struct CheersInputData
    {
        /// <summary>振りの速度</summary>
        public float Velocity;

        /// <summary>振りの角度</summary>
        public float Angle;

        /// <summary>検出時刻</summary>
        public float Timestamp;
    }

}
