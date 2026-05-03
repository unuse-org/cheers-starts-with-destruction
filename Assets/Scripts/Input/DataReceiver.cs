/**
 * DataReceiver.cs
 * SerialReceiver が受信した加速度データを GameObject の回転に反映するスクリプト
 *
 * 【使い方】
 * 1. 動かしたい GameObject にこのスクリプトをアタッチする
 * 2. Inspector で SensorId を設定する（マイコンの id と合わせる）
 * 3. SerialReceiver が同シーンに存在していれば自動で同期される
 *
 * 【同期内容】
 * 加速度 (x, y, z) [m/s²] → ピッチ・ロール → GameObject の rotation
 *   Roll  (Z軸回転) = atan2(y, z)
 *   Pitch (X軸回転) = atan2(-x, sqrt(y²+z²))
 */

using UnityEngine;
using CheersGame.Input;

public class DataReceiver : MonoBehaviour
{
    [Header("対象センサー設定")]
    [Tooltip("受信するマイコンの id（SerialReceiver の受信 JSON の id フィールドと合わせる）")]
    public int sensorId = 1;

    [Header("スムージング")]
    [Tooltip("0: 追従なし（即時反映）  0.99: ほぼ追従なし（ほぼ動かない）  推奨: 0.1〜0.3")]
    [Range(0f, 0.99f)]
    public float smoothing = 0.1f;

    [Header("デバッグ")]
    public bool showLog = false;

    // 現在の目標回転
    private Quaternion _targetRotation;
    private bool       _hasData = false;

    void OnEnable()
    {
        SerialReceiver.OnDataReceived += OnDataReceived;
        _targetRotation = transform.rotation;
    }

    void OnDisable()
    {
        SerialReceiver.OnDataReceived -= OnDataReceived;
    }

    void Update()
    {
        if (!_hasData) return;

        // スムージング適用（メインスレッドで補間）
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            _targetRotation,
            1f - smoothing
        );
    }

    // SerialReceiver.OnDataReceived イベントのハンドラ（メインスレッドから呼ばれる）
    private void OnDataReceived(SensorData data)
    {
        if (data.id != sensorId) return;

        // 加速度 → ピッチ・ロール変換
        float ax = data.x;
        float ay = data.y;
        float az = data.z;

        float roll  = Mathf.Atan2(ay, az) * Mathf.Rad2Deg;          // Z軸まわり
        float pitch = Mathf.Atan2(-ax, Mathf.Sqrt(ay * ay + az * az)) * Mathf.Rad2Deg; // X軸まわり

        _targetRotation = Quaternion.Euler(pitch, 0f, roll);
        _hasData = true;

        if (showLog)
            Debug.Log($"[DataReceiver] id:{data.id}  pitch:{pitch:F1}°  roll:{roll:F1}°");
    }
}
