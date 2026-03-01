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

        /// <summary>
        /// 音声入力が検出されたときに発火
        /// </summary>
        event Action<VoiceInputData> OnVoiceDetected;
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

    /// <summary>
    /// 音声入力データ
    /// </summary>
    public struct VoiceInputData
    {
        /// <summary>音量（0.0〜1.0の正規化値）</summary>
        public float Volume;

        /// <summary>検出時刻</summary>
        public float Timestamp;
    }
}
