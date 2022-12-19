using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TimeCycleManager : MonoBehaviour
{
    public static TimeCycleManager Instance { get; private set; }

    public enum SeasonType
    {
        Summer, Autumn, Winter, Spring
    }

    public enum WeatherType
    {
        Favorable, Bad
    }

    [Serializable]
    private class SeasonData
    {
        public TimeCycleData clearWeatherCycle;
        public TimeCycleData badWeatherCycle;
    }

    [SerializeField] private float seasonTransitionTime = 1f;
    [SerializeField] private float weatherTransitionTime = 1f;
    [SerializeField] private Light sun;
    [Space]
    public SeasonType currentSeason;
    public WeatherType currentWeather;
    [Space]
    [SerializeField] private SeasonData summerData;
    [SerializeField] private SeasonData autumnData;
    [SerializeField] private SeasonData winterData;
    [SerializeField] private SeasonData springData;

    public float CarAngleSmoothTimeFactor { get; private set; } = 1f;

    private float seasonFactor;
    private float weatherFactor;
    private float nextSeasonChangeTime;
    private float nextWeatherChangeTime;

    private float zOffset;

    private int puddlesFactorId;
    private int snowFactorId;
    private int foliageColorId;
    private int zOffsetId;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;
    }

    private void Start()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.fogMode = FogMode.Exponential;

        puddlesFactorId = Shader.PropertyToID("_PuddlesFactor");
        snowFactorId = Shader.PropertyToID("_SnowFactor");
        foliageColorId = Shader.PropertyToID("_FoliageColor");
        zOffsetId = Shader.PropertyToID("_OffsetZ");

        GameManager.Instance.OnLoaded += () =>
        {
            var levelData = GameManager.Instance.LevelData;
            currentSeason = levelData.startSeason.GetNextValueByChance();
            currentWeather = levelData.startWeather.GetNextValueByChance();

            seasonFactor = GetSeasonFactor(currentSeason);
            weatherFactor = GetWeatherFactor(currentWeather);

            nextSeasonChangeTime = CalculateNextSeasonChangeTime();
            nextWeatherChangeTime = CalculateNextWeatherChangeTime();
        };
        GameManager.Instance.OnGameStart += () =>
        {
            zOffset = Random.Range(0f, 1000f);
        };
    }

    private void Update()
    {
        if (!GameManager.Instance.IsLoaded)
            return;

        if (Time.time > nextSeasonChangeTime)
        {
            nextSeasonChangeTime = CalculateNextSeasonChangeTime();
            currentSeason = (SeasonType)(((int)currentSeason + 1) % 4);
        }

        if (Time.time > nextWeatherChangeTime)
        {
            nextWeatherChangeTime = CalculateNextWeatherChangeTime();
            currentWeather = GetRandomWeather();
        }

        zOffset += MapManager.Instance.MoveSpeed * Time.deltaTime;

        float targetSeasonFactor = GetSeasonFactor(currentSeason);

        seasonFactor = Mathf.MoveTowards(
            seasonFactor % 4f,
            (Mathf.Abs(targetSeasonFactor - seasonFactor) < 4f - seasonFactor + targetSeasonFactor) ? targetSeasonFactor : 4f,
            Time.deltaTime / seasonTransitionTime
        );

        var startSeason = GetSeasonData(Mathf.FloorToInt(seasonFactor));
        var endSeason = GetSeasonData((Mathf.FloorToInt(seasonFactor) + 1) % 4);

        var clearWeatherCycle = TimeCycleData.Lerp(
            startSeason.clearWeatherCycle,
            endSeason.clearWeatherCycle,
            Easings.EaseInOutCubic(seasonFactor % 1f)
        );
        var badWeatherCycle = TimeCycleData.Lerp(
            startSeason.badWeatherCycle,
            endSeason.badWeatherCycle,
            Easings.EaseInOutCubic(seasonFactor % 1f)
        );

        weatherFactor = Mathf.MoveTowards(
            weatherFactor,
            GetWeatherFactor(currentWeather),
            Time.deltaTime / weatherTransitionTime
        );

        var currTimeCycle = TimeCycleData.Lerp(
            clearWeatherCycle,
            badWeatherCycle,
            Easings.EaseInOutCubic(weatherFactor)
        );

        RenderSettings.ambientLight = currTimeCycle.ambientColor;
        RenderSettings.fog = !Mathf.Approximately(currTimeCycle.fogDensity, 0f);
        RenderSettings.fogDensity = currTimeCycle.fogDensity;
        RenderSettings.fogColor = currTimeCycle.fogColor;

        sun.color = currTimeCycle.sunColor;
        sun.transform.rotation = Quaternion.Euler(currTimeCycle.sunDirection);

        CarAngleSmoothTimeFactor = currTimeCycle.carAngleSmoothTimeFactor;

        Shader.SetGlobalFloat(zOffsetId, zOffset);
        Shader.SetGlobalColor(foliageColorId, currTimeCycle.foliageColor);
        Shader.SetGlobalFloat(puddlesFactorId, currTimeCycle.puddlesFactor);
        Shader.SetGlobalFloat(snowFactorId, currTimeCycle.snowFactor);
    }
    
    private SeasonData GetSeasonData(SeasonType seasonType)
    {
        return seasonType switch
        {
            SeasonType.Summer => summerData,
            SeasonType.Autumn => autumnData,
            SeasonType.Winter => winterData,
            _ => springData
        };
    }

    private SeasonData GetSeasonData(int seasonIndex)
    {
        return seasonIndex switch
        {
            0 => summerData,
            1 => autumnData,
            2 => winterData,
            _ => springData
        };
    }

    private static float GetSeasonFactor(SeasonType seasonType)
    {
        return seasonType switch
        {
            SeasonType.Summer => 0f,
            SeasonType.Autumn => 1f,
            SeasonType.Winter => 2f,
            SeasonType.Spring => 3f,
            _ => 0f
        };
    }

    private static float GetWeatherFactor(WeatherType weatherType)
    {
        return weatherType == WeatherType.Favorable ? 0f : 1f;
    }

    private float CalculateNextSeasonChangeTime()
    {
        var period = GameManager.Instance.LevelData.seasonChangePeriod;
        var randFactor = GameManager.Instance.LevelData.seasonChangePeriodRandomFactor;

        if (period > 0f)
            return Time.time + period * Random.Range(1f, 1f + randFactor);

        return float.MaxValue;
    }

    private float CalculateNextWeatherChangeTime()
    {
        var period = GameManager.Instance.LevelData.weatherChangePeriod;
        var randFactor = GameManager.Instance.LevelData.weatherChangePeriodRandomFactor;

        if (period > 0f)
            return Time.time + period * Random.Range(1f, 1f + randFactor);

        return float.MaxValue;
    }

    private static SeasonType GetRandomSeason() => (SeasonType)Random.Range(0, 4);

    private static WeatherType GetRandomWeather() => (WeatherType)Random.Range(0, 2);
}
