using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

[AddComponentMenu("Managers/Game Manager")]
public class GameManager : MonoBehaviour
{
    private class GameCounter
    {
        public float value;
        public FloatVariable globalVar;
        public GameModeCounter gameModeCounter;
    }

    // Ссылка на представителя класса одиночки
    public static GameManager Instance { get; private set; }

    public const string EndReasonCrash = "crash";
    public const string EndReasonTimeIsOver = "timeIsOver";
    public const string EndReasonRaceEnded = "raceEnded";
    public const string EndReasonChallengeCompleted = "challengeCompleted";

    private const string MainMenuSceneName = "MainMenu";
    private const string ScoreCounterName = "score";
    private const string PlayTimeCounterName = "playTime";
    private const string CoveredDistanceCounterName = "coveredDistance";
    private const string CountdownTimeVariableName = "countdownTime";
    private const string EndGameReasonVariableName = "endGameReason";
    private const string MoneyRewardVariableName = "moneyReward";
    private const string ReputationRewardVariableName = "reputationReward";
    private const string SparesRewardVariableName = "sparesReward";

    private const int PauseCountdownTime = 3;
    private const int GameStartCountdownTime = 3;
    private const float TimeTransitionTime = 0.35f;

    public event Action OnLoaded;
    public event Action OnGameStart;
    public event Action<string> OnGameEnd;
    public event Action OnPause;
    public event Action OnUnpause;
    public event Action<int, string> OnAddScore;
    public event Action<string, float> OnGameCounterUpdate;

    public LevelData LevelData { get; private set; }
    public GameModeConfig GameModeConfig { get; private set; }
    public CarConfig PlayerCarConfig { get; private set; }
    public string[] GameCountersNames { get; private set; }
    public LevelData.LevelTask[] LevelTasks => LevelData.tasks;
    public int CarLayerMask => LayerMask.GetMask("Car");
    public int CountdownTimer
    {
        get => countdownTimeVar.Value;
        private set => countdownTimeVar.Value = value;
    }
    public int Score {
        get => Mathf.FloorToInt(GetCounterValue(ScoreCounterName));
        private set => SetCounterValue(ScoreCounterName, value);
    }
    public float PlayTime
    {
        get => GetCounterValue(PlayTimeCounterName);
        private set => SetCounterValue(PlayTimeCounterName, value);
    }
    public float CoveredDistance
    {
        get => GetCounterValue(CoveredDistanceCounterName);
        private set => SetCounterValue(CoveredDistanceCounterName, value);
    }
    public int MoneyReward => moneyRewardVar.Value;
    public int ReputationReward => reputationRewardVar.Value;
    public int SparesReward => sparesRewardVar.Value;
    public bool IsGameStarted { get; private set; }
    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            if (CountdownTimer > 0 || !IsGameStarted || timeTransitionHandler != null)
                return;

            if (value && !_isPaused)
            {
                timeTransitionHandler = StartCoroutine(
                    MakeTimeScaleTransition(1f, 0f, TimeTransitionTime)
                );
                Time.timeScale = 0.0f;
                OnPause?.Invoke();
            }
            if (!value && _isPaused)
            {
                OnUnpause?.Invoke();
                StartCountdownTimer(PauseCountdownTime);
            }

            _isPaused = value;
        }
    }
    public bool IsLoaded { get; private set; } = false;

    private IntVariable countdownTimeVar;
    private StringVariable endGameReasonText;
    private IntVariable moneyRewardVar;
    private IntVariable reputationRewardVar;
    private IntVariable sparesRewardVar;
    private bool _isPaused = false;
    private Coroutine countdownHandler;
    private Coroutine timeTransitionHandler;
    private readonly Dictionary<string, GameCounter> gameModeCounters = new();

    private AsyncOperationHandle<GameModeConfig> gmConfigHandle;
    private AsyncOperationHandle<GameObject> playerCarHandle;

    private void Awake()
    {
        // Делаем класс одиночкой
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;
    }

    private void Start()
    {
        GameLoader.Instance.OnReady += () =>
        {
            SettingsManager.Instance.RefreshSettings();

            countdownTimeVar = LocalizationHelper.GetGlobalVariable<IntVariable>(CountdownTimeVariableName);
            endGameReasonText = LocalizationHelper.GetGlobalVariable<StringVariable>(EndGameReasonVariableName);
            moneyRewardVar = LocalizationHelper.GetGlobalVariable<IntVariable>(MoneyRewardVariableName);
            reputationRewardVar = LocalizationHelper.GetGlobalVariable<IntVariable>(ReputationRewardVariableName);
            sparesRewardVar = LocalizationHelper.GetGlobalVariable<IntVariable>(SparesRewardVariableName);

            LevelData = GameLoader.Instance.currentLevel;

            var playerCar = LevelData.playerCar;
            if (!playerCar.IsValid())
            {
                playerCar = CarsDataList.GetCarDataById(PlayerProgressManager.PlayerCarId).carConfigReference;
            }

            // Загружаем ассеты
            gmConfigHandle = LevelData.GetGameModeConfigHandle();
            playerCarHandle = playerCar.InstantiateAsync();
            // Ждем их загрузки
            gmConfigHandle.WaitForCompletion();

            GameModeConfig = gmConfigHandle.Result;

            // Создаем менеджера игрового режима
            if (GameModeConfig.gameModeManager != null)
            {
                Instantiate(GameModeConfig.gameModeManager, transform.parent);
            }

            var gmCounters = GameModeConfig.gameCounters;
            GameCountersNames = new string[gmCounters.Length];

            // Инициализируем игровые счетчики
            for (int i = 0; i < gmCounters.Length; i++)
            {
                var gmCounter = gmCounters[i];

                GameCountersNames[i] = gmCounter.name;

                var gameCounter = new GameCounter();
                gameModeCounters[gmCounter.name] = gameCounter;

                gameCounter.gameModeCounter = gmCounter;
                gameCounter.globalVar = LocalizationHelper.GetGlobalVariable<FloatVariable>(gmCounter.name);
                gameCounter.globalVar.Value = 0f;
            }

            StartCoroutine(OnStartCoroutine());
        };
    }

    private IEnumerator OnStartCoroutine()
    {
        // Ждем подгрузки всех ассетов
        yield return new WaitUntil(() => playerCarHandle.IsDone);
        PlayerCarConfig = playerCarHandle.Result.GetComponent<CarConfig>();

        IsLoaded = true;
        OnLoaded?.Invoke();
        StartGame();
    }

    private void Update()
    {
        if (IsGameStarted)
        {
            PlayTime += Time.deltaTime;
            CoveredDistance += MapManager.Instance.MoveSpeed * Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        Addressables.ReleaseInstance(gmConfigHandle);
        Addressables.ReleaseInstance(playerCarHandle);
    }

    private IEnumerator StartCountdownTimerCoroutine(int time)
    {
        CountdownTimer = time;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(0.2f);
        while (CountdownTimer > 0)
        {
            yield return new WaitForSecondsRealtime(1f);
            CountdownTimer -= 1;
        }

        countdownHandler = null;
        timeTransitionHandler = StartCoroutine(
            MakeTimeScaleTransition(0f, 1f, TimeTransitionTime)
        );
    }

    private IEnumerator ShowStartMessage()
    {
        yield return new WaitForSecondsRealtime(GameStartCountdownTime + 0.5f);
        if (LevelData.tasks.Length > 0)
        {
            var taskDescr = LevelData.tasks[0].GetLocalizedTaskText();
            UIMessage.Default.Show(taskDescr, UIMessage.BlackBackground, UIMessage.ShortDuration);
        }
    }

    private IEnumerator MakeTimeScaleTransition(float from, float to, float time)
    {
        var t = 0f;
        while (t < time)
        {
            Time.timeScale = Mathf.SmoothStep(from, to, t / time);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        Time.timeScale = to;
        timeTransitionHandler = null;
    }

    public void StartGame()
    {
        if (!IsGameStarted)
        {
            // Показываем подсказку о задании при запуске уровня (если необходимо)
            if (LevelData.showTaskHintOnStart)
            {
                StartCoroutine(ShowStartMessage());
            }

            if (countdownHandler != null)
                StopCoroutine(countdownHandler);

            if (timeTransitionHandler != null)
                StopCoroutine(timeTransitionHandler);

            CountdownTimer = 0;
            Time.timeScale = 0f;

            _isPaused = false;
            IsGameStarted = true;
            Score = 0;
            PlayTime = 0f;
            CoveredDistance = 0f;
            StartCountdownTimer(GameStartCountdownTime);
            OnGameStart?.Invoke();
        }
    }

    public void EndGame(string reason)
    {
        if (IsGameStarted)
        {
            if (countdownHandler != null)
                StopCoroutine(countdownHandler);

            if (timeTransitionHandler != null)
                StopCoroutine(timeTransitionHandler);

            CountdownTimer = 0;
            Time.timeScale = 1.0f;

            endGameReasonText.Value = LocalizationHelper.GetLocalizedString(reason);

            // Считаем награды, если заданий нет
            if (LevelTasks.Length == 0)
            {
                var counterVal = GetCounterValue(LevelData.rewardCounterName);

                int moneyReward = Mathf.FloorToInt(LevelData.moneyRewardFactor * counterVal); ;
                int reputationReward = Mathf.FloorToInt(LevelData.reputationRewardFactor * counterVal);
                int sparesReward = Mathf.FloorToInt(LevelData.sparesRewardFactor * counterVal);

                moneyRewardVar.Value = moneyReward;
                reputationRewardVar.Value = reputationReward;
                sparesRewardVar.Value = sparesReward;

                PlayerProgressManager.Money += moneyReward;
                PlayerProgressManager.ReputationScore += reputationReward;
                PlayerProgressManager.Spares += sparesReward;
            }
            else
            {
                int completedTask = LevelData.SaveData.maxCompletedTaskIndex;

                for (int i = completedTask + 1; i < LevelTasks.Length; i++)
                {
                    if (LevelData.IsTaskCompleted(i))
                    {
                        PlayerProgressManager.Money += LevelTasks[i].moneyReward;
                        PlayerProgressManager.ReputationScore += LevelTasks[i].reputationReward;
                        PlayerProgressManager.Spares += LevelTasks[i].sparesReward;
                    }
                }
            }

            LevelData.Save();

            _isPaused = false;
            IsGameStarted = false;
            OnGameEnd?.Invoke(reason);
        }
    }

    public void RestartGame()
    {
        IsGameStarted = false;
        StartGame();
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(MainMenuSceneName, LoadSceneMode.Single);
    }

    public void AddScore(int amount, string reason)
    {
        if (!IsGameStarted)
            return;

        if (amount <= 0)
        {
            Debug.LogWarning("You can't add non positive score's amount", this);
            return;
        }

        Score += amount;
        
        OnAddScore?.Invoke(amount, reason);
    }

    public void StartCountdownTimer(int time)
    {
        countdownHandler = StartCoroutine(StartCountdownTimerCoroutine(time));
    }

    public GameModeCounter GetCounterData(string name) => gameModeCounters[name].gameModeCounter;

    public float GetCounterValue(string name) => gameModeCounters[name].value;

    public void SetCounterValue(string name, float value)
    {
        gameModeCounters[name].value = value;
        OnGameCounterUpdate?.Invoke(name, value);
    }

    public void UpdateGlobalVariables()
    {
        foreach (var kv in gameModeCounters)
        {
            var gmCounter = kv.Value;
            gmCounter.globalVar.Value = gmCounter.value;
        }
    }
}
