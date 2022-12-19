using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class LevelSave
{
    public float recordValue = float.NaN;
    public int maxCompletedTaskIndex = -1;
}

[CreateAssetMenu(menuName = "Game/Level data", fileName = "Level")]
public class LevelData : ScriptableObject
{
    public enum CompareMethod
    {
        Bigger, Less
    }

    public enum LevelSeason
    {
        Summer, Autumn, Winter, Spring, Random
    }

    public enum LevelWeather
    {
        Favorable, Bad, Random
    }

    [Serializable]
    public class LevelTask
    {
        public const string TaskTextValueVariable = "value";
        public const string MoneyRewardValueVariable = "moneyReward";
        public const string ReputationRewardValueVariable = "reputationReward";
        public const string SparesRewardValueVariable = "sparesReward";

        private const string RewardTextLocale = "rewardText";

        public LocalizedString taskText;
        public string counterName;
        public float targetValue;
        public CompareMethod compareMethod;
        public int moneyReward;
        public int reputationReward;
        public int sparesReward;

        public bool CheckCondition()
        {
            var value = GameManager.Instance.GetCounterValue(counterName);
            if (compareMethod == CompareMethod.Bigger)
                return value >= targetValue;
            else
                return value <= targetValue;
        }

        public string GetLocalizedTaskText()
        {
            var localizedStr = LocalizationHelper.MakeStringClone(taskText);
            localizedStr[TaskTextValueVariable] = new FloatVariable()
            {
                Value = targetValue
            };
            return localizedStr.GetLocalizedString();
        }

        public string GetLocalizedRewardText()
        {
            return LocalizationHelper.GetLocalizedString(RewardTextLocale, new Dictionary<string, IVariable>()
            {
                { MoneyRewardValueVariable, new IntVariable() { Value = moneyReward } },
                { ReputationRewardValueVariable, new IntVariable() { Value = reputationReward } },
                { SparesRewardValueVariable, new IntVariable() { Value = sparesReward } }
            });
        }
    }

    public string mapName;
    public string gameModeName;
    public SerializableDictionary<string, float> gameModeParameters;
    [Header("Map data")]
    [Range(0f, 1f)] public float trafficDensity = 0f;
    [Range(0f, 1f)] public float trafficSpeed = 0f;
    [Space]
    public ChanceValueElector<TimeCycleManager.SeasonType> startSeason;
    public ChanceValueElector<TimeCycleManager.WeatherType> startWeather;
    [Tooltip("Set negative to make season unchangable")]
    public float seasonChangePeriod = -1f;
    public float seasonChangePeriodRandomFactor = 0f;
    [Tooltip("Set negative to make weather unchangable")]
    public float weatherChangePeriod = -1f;
    public float weatherChangePeriodRandomFactor = 0f;
    [Space]
    public bool showTaskHintOnStart = true;
    [Header("Player data")]
    public AssetReference playerCar;
    public Color playerCarColor;
    [Header("Tasks and rewards")]
    [Tooltip("It is used only when no tasks are provided")]
    public float moneyRewardFactor = 0f;
    public float reputationRewardFactor = 0f;
    public float sparesRewardFactor = 0f;
    public string rewardCounterName;
    [Tooltip("If no tasks are provided, game uses rewardFromScoreFactor to give rewards")]
    public LevelTask[] tasks;

    public string PreferencesKey => $"LevelSave_{name}";
    public LevelSave SaveData
    {
        get
        {
            if (_save == null)
            {
                try {
                    _save = JsonUtility.FromJson<LevelSave>(
                        PlayerPrefs.GetString(PreferencesKey, "{}")
                    );
                }
                catch (ArgumentException) {
                    _save = new LevelSave();
                }
            }
            return _save;
        }
    }
    
    private LevelSave _save;

    public string GetMapAssetName() => mapName + "Map";
    
    public AsyncOperationHandle<GameModeConfig> GetGameModeConfigHandle()
    {
        return Addressables.LoadAssetAsync<GameModeConfig>(gameModeName + "Config");
    }
    
    public bool IsTaskCompleted(int taskIndex)
    {
        return tasks[taskIndex].CheckCondition() && (taskIndex <= 0 || IsTaskCompleted(taskIndex - 1));
    }

    public void Save()
    {
        var gmConfigHandle = GetGameModeConfigHandle();
        gmConfigHandle.WaitForCompletion();

        for (int i = tasks.Length - 1; i >= 0; i--)
        {
            if (IsTaskCompleted(i))
            {
                if (i > SaveData.maxCompletedTaskIndex)
                    SaveData.maxCompletedTaskIndex = i;
                break;
            }
        }

        if (tasks.Length == 0 || SaveData.maxCompletedTaskIndex >= 0)
        {
            var recordCounterValue = GameManager.Instance.GetCounterValue(gmConfigHandle.Result.recordCounterName);
            if (float.IsNaN(SaveData.recordValue) ||
                (gmConfigHandle.Result.recordType == GameModeConfig.RecordType.Largest && recordCounterValue > SaveData.recordValue) ||
                (gmConfigHandle.Result.recordType == GameModeConfig.RecordType.Smallest && recordCounterValue < SaveData.recordValue))
            {
                SaveData.recordValue = recordCounterValue;
            }
        }

        Addressables.Release(gmConfigHandle);

        PlayerPrefs.SetString(PreferencesKey, JsonUtility.ToJson(SaveData));
    }
}
