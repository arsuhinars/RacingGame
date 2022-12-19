using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

[AddComponentMenu("Managers/Traffic Manager")]
public class TrafficManager : MonoBehaviour
{
    private const float TrafficDensityFactor = 0.2f;

    // Ссылка на представителя класса одиночки
    public static TrafficManager Instance { get; private set; }

    [Serializable]
    public class TrafficCarConfig
    {
        public AssetReference carConfigPrefab;
        // public CarConfig carConfigPrefab;
        public Color[] colors;

        [NonSerialized] public GameObject preloadedCarConfig;
        [NonSerialized] public ObjectPool<PoolObject> pool;
    }

    [Serializable]
    public struct RoadLane
    {
        public float position;
        public bool isOpposite;
    }

    public float AverageCarSize => _averageCarSize;
    public float AverageTrafficSpeed => _maxTrafficSpeed * trafficSpeedFactor;
    public float MaxTrafficDistance => _maxTrafficDistance;
    public float MinTrafficDistance => _minTrafficDistance;
    public float AverageRoadLanesWidth { get; private set; }

    [SerializeField] private CarController carPrefab;
    [SerializeField] private Material carBodyMaterial;
    [SerializeField] private ChanceValueElector<TrafficCarConfig> carsPrefabs;
    [SerializeField] private List<RoadLane> roadLanes;
    [SerializeField, Range(0f, 1f)] private float trafficDensity;
    [SerializeField, Range(0f, 1f)] private float trafficSpeedFactor;
    [SerializeField] private int maxCarsCount = 20;
    [SerializeField] private float _averageCarSize;
    [SerializeField] private float _maxTrafficSpeed;
    [SerializeField] private float _maxTrafficDistance;
    [SerializeField] private float _minTrafficDistance;

    private float spawnDistanceCounter = 0.0f;

    public int GetRoadLanesCount() => roadLanes.Count;

    public RoadLane GetRoadLane(int index)
    {
        return roadLanes[index];
    }

    public Rigidbody FindCarOnRoadLane(int index, float position)
    {
        var ray = new Ray()
        {
            direction = (roadLanes[index].isOpposite ? -1f : 1f) * Vector3.forward,
            origin = new Vector3(roadLanes[index].position, 0.1f, position)
        };

        Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, GameManager.Instance.CarLayerMask);

        return hitInfo.rigidbody;
    }

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
            trafficDensity = GameLoader.Instance.currentLevel.trafficDensity;
            trafficSpeedFactor = GameLoader.Instance.currentLevel.trafficSpeed;
        };

        for (int i = 0; i < carsPrefabs.Count; i++)
        {
            var prefab = carsPrefabs[i].value;

            // Создаем для каждого префаба свой пул
            prefab.pool = new ObjectPool<PoolObject>(
                () =>
                {
                    var car = Instantiate(carPrefab, transform);
                    var carConfigHandle = prefab.carConfigPrefab.InstantiateAsync(car.transform);

                    carConfigHandle.WaitForCompletion();

                    car.GetComponent<PoolObject>().pool = prefab.pool;
                    car.SetCarConfigInstance(carConfigHandle.Result.GetComponent<CarConfig>());
                    
                    return car.GetComponent<PoolObject>();
                },
                (obj) =>
                {
                    obj.gameObject.SetActive(true);
                    obj.isActiveInPool = true;

                    obj.CarController.GetCarConfig().BodyColor = prefab.colors[Random.Range(0, prefab.colors.Length)];
                },
                (obj) =>
                {
                    obj.isActiveInPool = false;
                    obj.gameObject.SetActive(false);
                },
                (obj) =>
                {
                    prefab.carConfigPrefab.ReleaseInstance(obj.CarController.GetCarConfig().gameObject);
                    Destroy(obj.gameObject);
                }
            );
        }

        // Сортируем дорожные полосы слева направо (по координате)
        roadLanes.Sort((l1, l2) => (int)Mathf.Sign(l1.position - l2.position));

        // Находим среднюю ширину дорожной полосы
        AverageRoadLanesWidth = 0.0f;
        for (int i = 1; i < roadLanes.Count; i++)
        {
            AverageRoadLanesWidth += roadLanes[i].position - roadLanes[i - 1].position;
        }
        AverageRoadLanesWidth /= roadLanes.Count - 1;
    }

    private void OnDestroy()
    {
        foreach (var prefab in carsPrefabs)
        {
            prefab.value.pool.Dispose();
        }
    }

    private void Update()
    {
        spawnDistanceCounter += Mathf.Max(AverageTrafficSpeed, MapManager.Instance.MoveSpeed) * Time.deltaTime;

        if (spawnDistanceCounter >= AverageCarSize * 1.5f)
        {
            spawnDistanceCounter = 0.0f;

            for (int i = 0; i < roadLanes.Count; i++)
            {
                // Спавним трафик с учетом плотности
                if (Random.Range(0.0f, 1.0f) < trafficDensity * TrafficDensityFactor)
                    SpawnCar(i);
            }
        }
    }

    private void SpawnCar(int laneIndex)
    {
        var carPrefab = carsPrefabs.GetNextValueByChance();
        if (carPrefab.pool.CountActive > maxCarsCount / carsPrefabs.Count)
            return;

        var carPos = Vector3.zero;
        carPos.x = roadLanes[laneIndex].position;

        if (roadLanes[laneIndex].isOpposite || MapManager.Instance.MoveSpeed > AverageTrafficSpeed || Random.Range(0, 2) == 0)
        {
            carPos.z = _maxTrafficDistance + AverageCarSize * 0.5f;
        }
        else
        {
            carPos.z = _minTrafficDistance - AverageCarSize * 0.5f;
        }

        bool canSpawn = !Physics.CheckBox(
            carPos,
            new Vector3(AverageRoadLanesWidth * 0.5f, 1.0f, AverageCarSize * 0.5f),
            Quaternion.identity,
            GameManager.Instance.CarLayerMask
        );

        if (!canSpawn)
        {
            return;
        }

        if (laneIndex != 0 && laneIndex != GetRoadLanesCount() - 1)
        {
            // Если машина спавнится не с краю
            carPos.x += Random.Range(-AverageRoadLanesWidth * 0.15f, AverageRoadLanesWidth * 0.15f);
        }
        else
        {
            carPos.x += Random.Range(-AverageRoadLanesWidth * 0.05f, AverageRoadLanesWidth * 0.05f);
        }

        var carObject = carPrefab.pool.Get();

        var car = carObject.CarController;
        car.isOncoming = roadLanes[laneIndex].isOpposite;
            
        carObject.GetComponent<TrafficController>().CurrentLaneIndex = laneIndex;
        carObject.transform.SetPositionAndRotation(
            carPos,
            Quaternion.Euler(0.0f, roadLanes[laneIndex].isOpposite ? 180.0f : 0.0f, 0.0f)
        );
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (roadLanes == null)
            return;

        Gizmos.color = Color.red;
        for (int i = 0; i < roadLanes.Count; i++)
        {
            Gizmos.DrawLine(
                new Vector3(roadLanes[i].position, 0.0f, MinTrafficDistance),
                new Vector3(roadLanes[i].position, 0.0f, MaxTrafficDistance)
            );
        }
    }
#endif
}
