using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PrecipitationController : MonoBehaviour
{
    [SerializeField] private float transitionSpeed = 1f;
    [Space]
    [SerializeField] private bool appearInFavorableWeaher;
    [SerializeField] private bool appearInBadWeaher;
    [Space]
    [SerializeField] private bool appearsInSummer;
    [SerializeField] private bool appearsInAutumn;
    [SerializeField] private bool appearsInWinter;
    [SerializeField] private bool appearsInSpring;
    [Space]
    [SerializeField] private float defaultEmissionMultiplier = 0f;
    [SerializeField] private float favorableWeatherEmmisionMultiplier;
    [SerializeField] private float badWeatherEmmisionMultiplier;

    private ParticleSystem particle;
    private ParticleSystem.EmissionModule particleEmission;

    private float currEmmisionMultiplier;
    private float lastUpdateTime;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        particleEmission = particle.emission;
        particleEmission.rateOverTimeMultiplier = 0f;
        particle.Clear();
    }

    private void OnEnable()
    {
        Update();
        particleEmission.rateOverTimeMultiplier = currEmmisionMultiplier;
        particle.Stop();
        particle.Clear();
        particle.Play();
    }

    private void Update()
    {
        float deltaTime = Time.time - lastUpdateTime;
        lastUpdateTime = Time.time;

        currEmmisionMultiplier = Mathf.MoveTowards(currEmmisionMultiplier, GetTargetEmmision(), transitionSpeed * deltaTime);

        //particleEmission.enabled = !Mathf.Approximately(currEmmisionMultiplier, 0f);
        particleEmission.rateOverTimeMultiplier = currEmmisionMultiplier;
    }

    private float GetTargetEmmision()
    {
        var currSeason = TimeCycleManager.Instance.currentSeason;
        var currWeather = TimeCycleManager.Instance.currentWeather;

        var isSuitableSeason = false;
        isSuitableSeason |= appearsInSummer && currSeason == TimeCycleManager.SeasonType.Summer;
        isSuitableSeason |= appearsInAutumn && currSeason == TimeCycleManager.SeasonType.Autumn;
        isSuitableSeason |= appearsInWinter && currSeason == TimeCycleManager.SeasonType.Winter;
        isSuitableSeason |= appearsInSpring && currSeason == TimeCycleManager.SeasonType.Spring;

        var isSuitableWeather = false;
        isSuitableWeather |= appearInFavorableWeaher && currWeather == TimeCycleManager.WeatherType.Favorable;
        isSuitableWeather |= appearInBadWeaher && currWeather == TimeCycleManager.WeatherType.Bad;

        if (isSuitableSeason && isSuitableWeather)
        {
            return TimeCycleManager.Instance.currentWeather == TimeCycleManager.WeatherType.Favorable ?
                favorableWeatherEmmisionMultiplier :
                badWeatherEmmisionMultiplier;
        }

        return defaultEmissionMultiplier;
    }
}
