using System;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace CheersGame.Input
{
    /// <summary>
    /// シリアル受信で使う生データ構造体
    /// </summary>
    [Serializable]
    internal class SensorRawData
    {
        public int    id;
        public string mac;
        public float  x;
        public float  y;
        public float  z;
        public int    count;
    }

    /// <summary>
    /// ISensorInput の実センサー実装。
    /// M5StickC Plus2 からシリアルでデータを受信し、乾杯イベントを発火する。
    /// </summary>
    public class RealSensorInput : MonoBehaviour, ISensorInput
    {
        // Inspector 設定
        [Header("シリアル設定")]
        [Tooltip("空欄にすると /dev/tty.usbserial* から自動検出します")]
        public string portName = "";
        public int baudRate = 115200;

        [Header("乾杯検出")]
        [Tooltip("乾杯と判定する加速度変化速度の閾値 [m/s²/s]  推奨: 3〜8")]
        public float cheersVelocityThreshold = 5.0f;

        [Header("デバッグ")]
        public bool showLog = true;

        /// <summary>乾杯アクションが検出されたときに発火</summary>
        public event Action<CheersInputData> OnCheersDetected;

        //　内部変数

        private SerialPort    _port;
        private Thread        _readThread;
        private volatile bool _running = false;

        // バックグラウンドスレッド → メインスレッド へのキュー
        private readonly Queue<SensorRawData> _queue = new Queue<SensorRawData>();
        private readonly object               _qLock  = new object();

        // 乾杯検出用: センサーIDごとの直前データ
        private readonly Dictionary<int, (float x, float y, float z, float timestamp)> _prevData
            = new Dictionary<int, (float, float, float, float)>();
        
        void Start()
        {
            OpenPort();
        }

        void Update()
        {
            // ---- キューを毎フレーム処理（メインスレッド） ----
            while (true)
            {
                SensorRawData raw = null;
                lock (_qLock)
                {
                    if (_queue.Count > 0) raw = _queue.Dequeue();
                }
                if (raw == null) break;

                float now = Time.realtimeSinceStartup;

                DetectCheers(raw, now);

                if (showLog)
                    Debug.Log($"[RealSensorInput] id:{raw.id}  X:{raw.x:F3}  Y:{raw.y:F3}  Z:{raw.z:F3}  cnt:{raw.count}");
            }
        }

        void OnDestroy()       => ClosePort();
        void OnApplicationQuit() => ClosePort();

        
        // 乾杯検出
        private void DetectCheers(SensorRawData raw, float now)
        {
            int id = raw.id;

            if (!_prevData.TryGetValue(id, out var prev))
            {
                _prevData[id] = (raw.x, raw.y, raw.z, now);
                return;
            }

            float dt = now - prev.timestamp;
            if (dt <= 0f)
            {
                _prevData[id] = (raw.x, raw.y, raw.z, now);
                return;
            }

            float dx = raw.x - prev.x;
            float dy = raw.y - prev.y;
            float dz = raw.z - prev.z;
            float velocity = Mathf.Sqrt(dx * dx + dy * dy + dz * dz) / dt;

            if (velocity > cheersVelocityThreshold)
            {
                Vector3 v        = new Vector3(dx, dy, dz) / dt;
                float   angleRad = Mathf.Acos(Mathf.Clamp(Vector3.Dot(v.normalized, Vector3.up), -1f, 1f));
                float   angleDeg = angleRad * Mathf.Rad2Deg;

                OnCheersDetected?.Invoke(new CheersInputData
                {
                    Velocity  = velocity,
                    Angle     = angleDeg,
                    Timestamp = now
                });

                if (showLog)
                    Debug.Log($"[RealSensorInput] Cheers detected  id:{id}  v:{velocity:F2}  ang:{angleDeg:F1}°");
            }

            _prevData[id] = (raw.x, raw.y, raw.z, now);
        }

        // シリアルポート開閉
        private void OpenPort()
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            var ports = new List<string>(System.IO.Directory.GetFiles("/dev", "cu.*"));
#else
            var ports = new List<string>(SerialPort.GetPortNames());
#endif
            Debug.Log("[RealSensorInput] Available ports: " + string.Join(", ", ports));

            string target = string.IsNullOrEmpty(portName) ? DetectPort(ports) : portName;
            if (target == null)
            {
                Debug.LogError("[RealSensorInput] No valid port found.");
                return;
            }

            try
            {
                _port = new SerialPort(target, baudRate)
                {
                    ReadTimeout  = 1000,
                    WriteTimeout = 1000,
                    NewLine      = "\n"
                };
                _port.Open();
                portName    = target;
                _running    = true;
                _readThread = new Thread(ReadLoop) { IsBackground = true };
                _readThread.Start();
                Debug.Log($"[RealSensorInput] Opened: {target} @ {baudRate}bps");
            }
            catch (Exception e)
            {
                Debug.LogError($"[RealSensorInput] Failed to open {target}: {e.Message}");
            }
        }

        private void ClosePort()
        {
            _running = false;
            _readThread?.Join(500);
            if (_port != null && _port.IsOpen)
            {
                _port.Close();
                Debug.Log("[RealSensorInput] Port closed.");
            }
        }

        /// <summary>/dev/tty.usbserial* の中から JSON が届くポートを自動検出する</summary>
        private string DetectPort(List<string> ports)
        {
            // /dev/tty.usbserial -> /dev/cu.* に変わるケースやM5Stickの名前を考慮
            foreach (string p in ports)
            {
                if (!p.Contains("usbserial") && !p.Contains("M5StickCPlus")) continue;

                Debug.Log($"[RealSensorInput] Probing: {p}");
                SerialPort probe = null;
                try
                {
                    probe = new SerialPort(p, baudRate) { ReadTimeout = 2000, NewLine = "\n" };
                    probe.Open();

                    for (int i = 0; i < 3; i++)
                    {
                        string line = probe.ReadLine().Trim();
                        if (line.StartsWith('{') && line.Contains("\"id\""))
                        {
                            Debug.Log($"[RealSensorInput] Auto-detected: {p}");
                            return p;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"[RealSensorInput] Probe failed {p}: {e.Message}");
                }
                finally
                {
                    if (probe != null && probe.IsOpen) probe.Close();
                }
            }
            return null;
        }

        /// <summary>利用可能なシリアルポート名を返す。エディタ UI 等から利用可能。</summary>
        public static string[] GetAvailablePortNames()
        {
            try   { return SerialPort.GetPortNames(); }
            catch (Exception e)
            {
                Debug.LogWarning($"[RealSensorInput] GetAvailablePortNames failed: {e.Message}");
                return Array.Empty<string>();
            }
        }

        // 受信スレッド（バックグラウンド）
        private void ReadLoop()
        {
            while (_running)
            {
                try
                {
                    string line = _port.ReadLine().Trim();
                    if (string.IsNullOrEmpty(line) || !line.StartsWith("{")) continue;

                    SensorRawData raw = JsonUtility.FromJson<SensorRawData>(line);
                    if (raw == null) continue;

                    lock (_qLock) { _queue.Enqueue(raw); }
                }
                catch (TimeoutException)
                {
                    // タイムアウトは無視して継続
                }
                catch (Exception e)
                {
                    if (_running)
                        Debug.LogWarning($"[RealSensorInput] Read error: {e.Message}");
                }
            }
        }
    }
}
