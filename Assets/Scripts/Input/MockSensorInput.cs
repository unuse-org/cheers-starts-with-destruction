using System;
using UnityEngine;

namespace CheersGame.Input
{
    /// <summary>
    /// テスト用のモックセンサー入力。
    /// キーボード入力で乾杯アクションをシミュレートする。
    /// </summary>
    public class MockSensorInput : MonoBehaviour, ISensorInput
    {
        public event Action<CheersInputData> OnCheersDetected;

        private void Update()
        {
            // Space: 通常の乾杯（中程度の速度・正面角度）
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                FireCheers(velocity: 1.0f, angle: 0f);
            }

            // 1〜5: 異なる強度のプリセット
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                FireCheers(velocity: 0.3f, angle: 0f);   // 弱い
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                FireCheers(velocity: 0.6f, angle: 5f);   // やや弱い
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                FireCheers(velocity: 1.0f, angle: 0f);   // 普通
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4))
            {
                FireCheers(velocity: 1.5f, angle: -5f);  // 強い
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha5))
            {
                FireCheers(velocity: 2.0f, angle: -10f); // 最強
            }

            // R: ランダムな入力
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                float randomVelocity = UnityEngine.Random.Range(0.1f, 2.5f);
                float randomAngle = UnityEngine.Random.Range(-45f, 45f);
                FireCheers(randomVelocity, randomAngle);
            }
        }

        private void FireCheers(float velocity, float angle)
        {
            var data = new CheersInputData
            {
                Velocity = velocity,
                Angle = angle,
                Timestamp = Time.time,
            };

            Debug.Log($"[MockSensor] Cheers! Velocity={velocity:F2}, Angle={angle:F1}");
            OnCheersDetected?.Invoke(data);
        }
    }
}
