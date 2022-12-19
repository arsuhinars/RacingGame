using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class PoliceManager : MonoBehaviour
{
    public static PoliceManager Instance { get; private set; }

    /*public enum OffenceType
    {
        OverSpeed, DangerousRide, PropertyDamage, Crash
    }*/

    /*[Serializable]
    private class PenaltyData
    {
        public float chasePoints;
        public float delay;

        [NonSerialized] public float lastPenaltyTime;
    } */

    /*[Serializable]
    private class ChaseLevelData
    {
        public float minChasePoints;
        public int maxChasingCarsAmount;
    }*/

    private class PoliceCar
    {
        public CarController carController;
        public PoliceController policeController;
        public GameObject gameObject;
    }

    public bool IsPoliceChasing { get; private set; } = false;
    // public float ChasePoints { get; private set; } = 0f;
    
    [SerializeField] private PoliceController policeCarPrefab;
    // [SerializeField] private SerializableDictionary<OffenceType, PenaltyData> penalties = new();
    [Space]
    [SerializeField] private float minCarSpawnDelay;
    [SerializeField] private float maxCarSpawnDelay;
    [Space]
    [SerializeField, Range(0f, 1f)] private float chaseStartChance;
    [SerializeField] private float chaseTimerTick;
    [SerializeField] private int maxCarCount;
    // [SerializeField] private ChaseLevelData[] chaseLevels;
    // [SerializeField] private float maxChasePoints;
    
    private ObjectPool<PoliceCar> carsPool;

    private float carSpawnTimer;
    private float chaseTimer;

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
        carsPool = new(
            () =>
            {
                var policeController = Instantiate(policeCarPrefab, transform);
                return new PoliceCar()
                {
                    policeController = policeController,
                    gameObject = policeController.gameObject,
                    carController = policeController.GetComponent<CarController>()
                };
            }
        );
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameStarted)
        {
            return;
        }

        /* if (IsPoliceChasing)
        {
            ChasePoints -= chasePointsDecreaseSpeed * Time.deltaTime;
            if (ChasePoints <= 0f)
            {
                ChasePoints = 0f;
                IsPoliceChasing = false;
            }
            ChasePoints = Mathf.Clamp(ChasePoints, 0f, maxChasePoints);
        }
        else
        {
            ChasePoints = 0f;
        }*/

        chaseTimer += Time.deltaTime;
        if (chaseTimer > chaseTimerTick)
        {
            chaseTimer = 0f;
            if (Random.value < chaseStartChance && IsPoliceChasing)
            {
                IsPoliceChasing = true;
            }
        }

        carSpawnTimer -= Time.deltaTime;
        if (carSpawnTimer < 0f)
        {
            carSpawnTimer = Random.Range(minCarSpawnDelay, maxCarSpawnDelay);
            TryToSpawnChasingCar();
        }
    }

    private void TryToSpawnChasingCar()
    {
        if (carsPool.CountActive >= maxCarCount)
            return;

        var policeCar = carsPool.Get();

        // Ищем дорогу, на которой трафик дальше всего от места спавна
        int laneIndex = 0;
        var laneDistance = 0f;
        var checkOrigin = TrafficManager.Instance.MinTrafficDistance;
        for (int i = 0; i < TrafficManager.Instance.GetRoadLanesCount(); i++)
        {
            var rb = TrafficManager.Instance.FindCarOnRoadLane(laneIndex, checkOrigin);
            var dist = rb != null ? rb.transform.position.z - checkOrigin : Mathf.Infinity;
            if (dist > laneDistance)
            {
                laneDistance = dist;
                laneIndex = i;
            }
        }
        var spawnPos = new Vector3(
            TrafficManager.Instance.GetRoadLane(laneIndex).position,
            0f,
            TrafficManager.Instance.MinTrafficDistance - TrafficManager.Instance.AverageCarSize * 0.5f
        );
        policeCar.gameObject.transform.position = spawnPos;

        // Ищем положение для машины
        /*foreach (var kv in busyCarPosition)
        {
            if (!kv.Value)
            {
                policeCar.policeController.Position = kv.Key;
                busyCarPosition[kv.Key] = true;
                break;
            }
        }*/

        policeCar.gameObject.SetActive(true);
    }

    /*public void HandleOffence(OffenceType offenceType, float scale=1f)
    {
        try
        {
            var penalty = penalties[offenceType];
            if (Time.time - penalty.lastPenaltyTime > penalty.delay)
            {
                if (!IsPoliceChasing && Random.value < chaseChance)
                {
                    IsPoliceChasing = true;
                    ChasePoints = chaseLevels[0].minChasePoints;
                    return;
                }

                ChasePoints += penalty.chasePoints * scale;
                penalty.lastPenaltyTime = Time.time;
            }
        }
        catch (KeyNotFoundException)
        {
            Debug.LogError($"Penalty type {offenceType} is not provided", this);
        }
    }*/
}
