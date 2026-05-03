/**
 * SerialReceiver.cs
 * Unity でシリアルポートから M5StickC Plus2 の加速度データを受信するスクリプト
 *
 * 【使い方】
 * 1. このスクリプトを任意の GameObject にアタッチする
 * 2. Inspector で PortName を設定する（例: "COM3" / "/dev/tty.usbserial-XXXX"）
 * 3. Play するとデータ受信開始
 * 4. 他のスクリプトから SensorDataManager.GetData(id) でデータを取得できる
 *
 * 【受信フォーマット】
 * {"id":1,"mac":"AA:BB:CC:DD:EE:FF","x":0.1234,"y":-0.4567,"z":9.7891,"count":42}
 *
 * 【アーキテクチャ】
 * M5StickC Plus2 (ESP-NOW 受信機)
 *   ↓ USB シリアル（JSON 1行/パケット）
 * ReadLoop()  ← バックグラウンドスレッド（Unity API 不可）
 *   ↓ _queue.Enqueue()  ※ lock で排他制御
 * Update()    ← メインスレッド（毎フレーム）
 *   ↓ _queue.Dequeue() → タイムスタンプ付与
 *   ↓ SensorDataManager.Set()   → 他スクリプトから GetData(id) で参照可能
 *   ↓ OnDataReceived イベント発火 → 購読スクリプトへ通知
 */

using System;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace CheersGame.Input
{

// =============================================
// データ構造体
// =============================================
    [Serializable]
    public class SensorData
    {
        public int    id;
        public string mac;
        public float  x;
        public float  y;
        public float  z;
        public int    count;
        public float  timestamp; // 受信時の Time.realtimeSinceStartup
    }

// =============================================
// グローバルデータ管理（他スクリプトからアクセス用）
// =============================================
    public static class SensorDataManager
    {
        private static readonly Dictionary<int, SensorData> _store
            = new Dictionary<int, SensorData>();
        private static readonly object _lock = new object();

        public static void Set(SensorData d)
        {
            lock (_lock) { _store[d.id] = d; }
        }

        /// <summary>指定 id の最新データを取得。存在しない場合は null</summary>
        public static SensorData GetData(int id)
        {
            lock (_lock)
            {
                return _store.TryGetValue(id, out var d) ? d : null;
            }
        }

        /// <summary>受信済みすべてのデータを取得</summary>
        public static Dictionary<int, SensorData> GetAll()
        {
            lock (_lock) { return new Dictionary<int, SensorData>(_store); }
        }
    }

// =============================================
// シリアル受信コンポーネント
// =============================================
    public class SerialReceiver : MonoBehaviour, ISensorInput
{
    [Header("シリアル設定")]
    [Tooltip("Windowsは 'COM3' など、Macは '/dev/tty.usbserial-XXXX'")]
    public string portName    = "COM3";
    public int    baudRate    = 115200;

    [Header("デバッグ")]
    public bool showLog       = true;

        // イベント: データ受信時に外部から購読できる
        public static event Action<SensorData> OnDataReceived;

        // ISensorInput イベント: 乾杯 / 音声検出を購読するためのインスタンスイベント
        public event Action<CheersInputData> OnCheersDetected;
        public event Action<VoiceInputData> OnVoiceDetected;

    // --------------- 内部変数 ---------------
    private SerialPort   _port;
    private Thread       _thread;
    private volatile bool _running = false;

        // スレッド → メインスレッド へのキュー
        private readonly Queue<SensorData> _queue = new Queue<SensorData>();
        private readonly object            _qLock = new object();

        // 直前の受信データを保持して乾杯動作を検出する
        private readonly Dictionary<int, SensorData> _prevData = new Dictionary<int, SensorData>();

    // =============================================
    // Unity ライフサイクル
    // =============================================
    void Start()
    {
        OpenPort();
    }

    void Update()
    {
        // キューからメインスレッドで取り出して処理
        while (true)
        {
            SensorData d = null;
            lock (_qLock)
            {
                if (_queue.Count > 0) d = _queue.Dequeue();
            }
            if (d == null) break;

            // タイムスタンプはメインスレッドで付与
            d.timestamp = Time.realtimeSinceStartup;

            SensorDataManager.Set(d);
            OnDataReceived?.Invoke(d);

            // 簡易的な乾杯検出：前回データとの差分から瞬時速度を算出し閾値越えで発火
            if (_prevData.TryGetValue(d.id, out var prev))
            {
                float dt = d.timestamp - prev.timestamp;
                if (dt > 0f)
                {
                    float dx = d.x - prev.x;
                    float dy = d.y - prev.y;
                    float dz = d.z - prev.z;
                    float dv = Mathf.Sqrt(dx * dx + dy * dy + dz * dz) / dt;

                    const float CHEERS_VELOCITY_THRESHOLD = 5.0f;
                    if (dv > CHEERS_VELOCITY_THRESHOLD)
                    {
                        Vector3 v = new Vector3(dx, dy, dz) / dt;
                        float angleRad = Mathf.Acos(Mathf.Clamp(Vector3.Dot(v.normalized, Vector3.up), -1f, 1f));
                        float angleDeg = angleRad * Mathf.Rad2Deg;

                        CheersInputData ci = new CheersInputData
                        {
                            Velocity = dv,
                            Angle = angleDeg,
                            Timestamp = d.timestamp
                        };

                        OnCheersDetected?.Invoke(ci);
                        if (showLog)
                            Debug.Log($"[Serial] Cheers detected id:{d.id} v:{dv:F2} ang:{angleDeg:F1}");
                    }
                }
            }

            _prevData[d.id] = d;

            if (showLog)
                Debug.Log($"[Serial] Sender#{d.id}  X:{d.x:F3}  Y:{d.y:F3}  Z:{d.z:F3}  cnt:{d.count}");
        }
    }

    void OnDestroy()
    {
        ClosePort();
    }

    void OnApplicationQuit()
    {
        ClosePort();
    }

    // =============================================
    // ポート開閉
    // =============================================
    void OpenPort()
    {
        try
        {
            _port = new SerialPort(portName, baudRate)
            {
                ReadTimeout  = 1000,
                WriteTimeout = 1000,
                NewLine      = "\n"
            };
            _port.Open();
            _running = true;
            _thread  = new Thread(ReadLoop) { IsBackground = true };
            _thread.Start();
            Debug.Log($"[Serial] Opened: {portName} @ {baudRate}bps");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Serial] Failed to open {portName}: {e.Message}");
        }
    }

    void ClosePort()
    {
        _running = false;
        _thread?.Join(500);
        if (_port != null && _port.IsOpen)
        {
            _port.Close();
            Debug.Log("[Serial] Port closed.");
        }
    }

    // =============================================
    // 受信スレッド
    // =============================================
    void ReadLoop()
    {
        while (_running)
        {
            try
            {
                string line = _port.ReadLine().Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // JSON 行のみ処理（'{'で始まる行）
                if (!line.StartsWith("{")) continue;

                SensorData d = JsonUtility.FromJson<SensorData>(line);
                if (d == null) continue;

                lock (_qLock) { _queue.Enqueue(d); }
            }
            catch (TimeoutException)
            {
                // タイムアウトは無視して継続
            }
            catch (Exception e)
            {
                if (_running)
                    Debug.LogWarning($"[Serial] Read error: {e.Message}");
            }
        }
    }
}
}