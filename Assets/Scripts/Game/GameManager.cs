using System;
using System.Collections;
using UnityEngine;
using CheersGame.Input;
using CheersGame.Data;
using CheersGame.UI;

namespace CheersGame.Game
{
    public enum GameState
    {
        Title,
        Game,
        Score,
    }

    /// <summary>
    /// ゲーム全体の状態遷移を管理する。
    /// シングルシーン方式で、UIパネルのSetActive切り替えにより画面遷移を実現する。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MonoBehaviour _sensorInputComponent;
        [SerializeField] private PlayerGlass _playerGlass;
        [SerializeField] private GlassData _initialGlassData;
        [SerializeField] private NPCController _npcController;
        [SerializeField] private NPCData[] _npcDataList;

        [Header("Screens")]
        [SerializeField] private GameObject _titleScreen;
        [SerializeField] private GameObject _gameScreen;
        [SerializeField] private GameObject _scoreScreen;

        public GameState CurrentState { get; private set; }
        public int DefeatCount { get; private set; }
        public NPCData CurrentNPC { get; private set; }

        /// <summary>状態遷移時に発火（新しい状態を通知）</summary>
        public event Action<GameState> OnStateChanged;

        /// <summary>撃破数変化時に発火（現在の撃破数を通知）</summary>
        public event Action<int> OnDefeatCountChanged;

        /// <summary>NPC変更時に発火（新しいNPCDataを通知）</summary>
        public event Action<NPCData> OnNPCChanged;

        private ISensorInput _sensorInput;
        private TitleUI _titleUI;
        private GameUI _gameUI;
        private bool _isTransitioning;

        private void Awake()
        {

            _sensorInput = _sensorInputComponent as ISensorInput;
            if (_sensorInput == null)
                Debug.LogError("[GameManager] SensorInputComponent does not implement ISensorInput.");

            if (_titleScreen != null)
                _titleUI = _titleScreen.GetComponent<TitleUI>();
            if (_gameScreen != null)
                _gameUI = _gameScreen.GetComponent<GameUI>();
        }

        private void OnEnable()
        {
            if (_sensorInput != null)
            {
                _sensorInput.OnCheersDetected += HandleCheersInput;
            }

            if (_playerGlass != null)
            {
                _playerGlass.OnGlassBroken += HandleGlassBroken;
            }
        }

        private void OnDisable()
        {
            if (_sensorInput != null)
            {
                _sensorInput.OnCheersDetected -= HandleCheersInput;
            }

            if (_playerGlass != null)
            {
                _playerGlass.OnGlassBroken -= HandleGlassBroken;
            }
        }

        private void Start()
        {
            TransitionTo(GameState.Title);
            FindObjectOfType<BGMManager>().ChangeBGM(BGMManager.GameState.Title);
        }

#if UNITY_EDITOR
        private void Update()
        {
            // デバッグ用: Escapeキーでタイトルに戻る
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) && CurrentState != GameState.Title)
            {
                Debug.Log("[GameManager] Debug: Force return to Title");
                TransitionTo(GameState.Title);
            }
        }
#endif

        private void HandleCheersInput(CheersInputData data)
        {
            switch (CurrentState)
            {
                case GameState.Title:
                    if (!_isTransitioning)
                        StartCoroutine(StartGameWithTransition());
                    break;

                case GameState.Score:
                    TransitionTo(GameState.Title);
                    break;
            }
        }

        private void HandleGlassBroken()
        {
            if (CurrentState == GameState.Game)
                StartCoroutine(GameOverRoutine());
        }

        private IEnumerator GameOverRoutine()
        {
            if (_gameUI != null)
                yield return _gameUI.PlayGameOverOverlay();
            TransitionTo(GameState.Score);
        }

        private IEnumerator StartGameWithTransition()
        {
            _isTransitioning = true;
            if (_titleUI != null)
                yield return _titleUI.PlayStartAnimation();
            // Screenの遷移時間
            yield return new WaitForSeconds(1f);
            _isTransitioning = false;
            StartGame();
        }

        private void StartGame()
        {
            DefeatCount = 0;

            if (_playerGlass != null && _initialGlassData != null)
            {
                _playerGlass.Initialize(_initialGlassData);
            }

            TransitionTo(GameState.Game);

            //BGMManagerを探してBGM変更(ゲーム中BGMに変更)
            // FindObjectOfType<BGMManager>().ChangeBGM(BGMManager.GameState.Main);
            SpawnNextNPC();
        }

        /// <summary>
        /// NPCをランダム選択して初期化し、カウントダウンを開始する。
        /// </summary>
        public void SpawnNextNPC()
        {
            if (_npcController == null || _npcDataList == null || _npcDataList.Length == 0)
            {
                Debug.LogWarning("[GameManager] NPCController or NPCDataList is not configured.");
                return;
            }

            NPCData data = _npcDataList[UnityEngine.Random.Range(0, _npcDataList.Length)];
            CurrentNPC = data;
            _npcController.Initialize(data);
            Debug.Log($"[GameManager] Spawned NPC: {data.NPCName}");
            OnNPCChanged?.Invoke(data);
            _npcController.StartCheersSequence();
        }

        /// <summary>
        /// NPC撃破時に呼ばれる。BattleManagerから呼び出す想定。
        /// </summary>
        public void AddDefeat()
        {
            DefeatCount++;
            Debug.Log($"[GameManager] DefeatCount={DefeatCount}");
            OnDefeatCountChanged?.Invoke(DefeatCount);
        }

        private void TransitionTo(GameState newState)
        {
            // Game状態から離れる場合、カウントダウンを中断
            if (CurrentState == GameState.Game && newState != GameState.Game)
            {
                if (_npcController != null)
                {
                    _npcController.CancelCheersSequence();
                }
            }

            CurrentState = newState;
            Debug.Log($"[GameManager] State -> {newState}");

            SetScreenActive(_titleScreen, newState == GameState.Title);
            SetScreenActive(_gameScreen, newState == GameState.Game);
            SetScreenActive(_scoreScreen, newState == GameState.Score);

            // Debug.Log("BGM change to: " + newState);

            //追加（BGM制御）
            var bgm = FindObjectOfType<BGMManager>();
            if (bgm != null)
            {
                switch (newState)
                {
                    case GameState.Title:
                        bgm.ChangeBGM(BGMManager.GameState.Title);
                        break;
                    case GameState.Game:
                        bgm.ChangeBGM(BGMManager.GameState.Game);
                        AudioFeedback.Instance.PlaySE(AudioFeedback.SEType.Start);
                        break;
                    case GameState.Score:
                        bgm.ChangeBGM(BGMManager.GameState.Score);
                        break;
                }
            }

            OnStateChanged?.Invoke(newState);
        }

        private static void SetScreenActive(GameObject screen, bool active)
        {
            if (screen != null)
            {
                screen.SetActive(active);
            }
        }
    }
}
