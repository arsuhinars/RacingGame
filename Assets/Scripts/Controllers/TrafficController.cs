using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PoolObject))]
[RequireComponent(typeof(CarController))]
public class TrafficController : MonoBehaviour
{
    private const float FrontCarUpdateDelay = 1f/10f;
    private const float EvaporationDelay = 5f;
    private const float EvaporationTime = 1.5f;

    public bool IsOncoming => car.isOncoming;
    public float Speed => car.Speed;
    public float TargetSpeed 
    {
        get => targetSpeed;
        set
        {
            targetSpeed = value;
            targetGasValue = value / car.GetCarConfig().maxSpeed;
        }
    }
    public int CurrentLaneIndex
    {
        get => currLaneIndex;
        set
        {
            currLaneIndex = value;
        }
    }

    // Параметры в инспекторе
    [SerializeField, Range(0.1f, 2f)] private float maxSpeedFactor = 1f;
    [SerializeField, Range(0.1f, 2f)] private float minSpeedFactor = 1f;
    [SerializeField] private float minAdjustDistance = 14f;
    [SerializeField] private float maxAdjustDistance = 20f;

    private CarController car;
    private CarController frontCar;
    private Rigidbody rb;
    private PoolObject poolObject;
    private int currLaneIndex;
    private float frontCarUpdateTimer;
    private float targetSpeed;
    private float targetGasValue;
    private float evaporationTimer;

    private void Start()
    {
        car = GetComponent<CarController>();
        rb = GetComponent<Rigidbody>();
        poolObject = GetComponent<PoolObject>();

        GameManager.Instance.OnGameStart += () =>
        {
            if (poolObject.isActiveInPool)
                poolObject.ReturnToPool();
        };
    }

    private void OnEnable()
    {
        StartCoroutine(OnSpawnCoroutine());
    }

    private void OnDisable()
    {
        transform.position = Vector3.zero;
    }

    private IEnumerator OnSpawnCoroutine()
    {
        yield return null;

        TargetSpeed = Random.Range(minSpeedFactor, maxSpeedFactor) * TrafficManager.Instance.AverageTrafficSpeed;

        car.ResetState();
        car.Speed = TargetSpeed;
        car.gasValue = targetGasValue;
        car.turnValue = 0.0f;
        car.isWorking = true;
        car.GetCarConfig().Opacity = 1f;
        car.GetCarConfig().gameObject.layer = LayerMask.NameToLayer("Car");
    }

    private void Update()
    {
        // Возвращаем в пул, если зашли за границы
        if (transform.position.z + TrafficManager.Instance.AverageCarSize * 0.5f < TrafficManager.Instance.MinTrafficDistance ||
            transform.position.z - TrafficManager.Instance.AverageCarSize * 0.5f > TrafficManager.Instance.MaxTrafficDistance)
        {
            poolObject.ReturnToPool();
        }

        // Испаряем автомобиль, если тот попал в аварию
        if (!car.isWorking)
        {
            if (evaporationTimer >= EvaporationTime &&
                evaporationTimer - Time.deltaTime < EvaporationTime)
            {
                // Меняем слой машины, когда она начинает испаряться
                car.GetCarConfig().gameObject.layer = LayerMask.NameToLayer("EvaporatingCar");
            }

            evaporationTimer -= Time.deltaTime;
            if (evaporationTimer < 0f)
            {
                poolObject.ReturnToPool();
            }
            else if (evaporationTimer < EvaporationTime)
            {
                car.GetCarConfig().Opacity = evaporationTimer / EvaporationTime;
            }
            return;
        }

        // Подстраиваемся под скорость впереди едущей машины
        if (frontCar != null)
        {
            float distance = Mathf.Abs(transform.position.z - frontCar.transform.position.z);
            car.gasValue = Mathf.Lerp(
                frontCar.Speed / car.GetCarConfig().maxSpeed,
                targetGasValue,
                (distance - minAdjustDistance) / (maxAdjustDistance - minAdjustDistance)
            );
        }
        else car.gasValue = targetGasValue;

        // Обновляем впереди едущую машину
        frontCarUpdateTimer += Time.deltaTime;
        if (frontCarUpdateTimer > FrontCarUpdateDelay)
        {
            frontCarUpdateTimer = 0.0f;
            UpdateFrontCar();
        }
    }

    private void UpdateFrontCar()
    {
        // Находим позицию впереди нашего авто
        float pos = transform.position.z;
        if (IsOncoming)
            pos -= TrafficManager.Instance.AverageCarSize * 0.6f;
        else
            pos += TrafficManager.Instance.AverageCarSize * 0.6f;

        var rb = TrafficManager.Instance.FindCarOnRoadLane(currLaneIndex, pos);

        // Проверяем впереди едущую машину, если она достаточно близка
        if (rb != null && Mathf.Abs(transform.position.z - rb.position.z) <= maxAdjustDistance)
        {
            frontCar = rb.GetComponent<CarController>();
        }
        else frontCar = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        float deltaSpeed = collision.impulse.z / rb.mass * 0.5f;

        // Если произошла авария
        if (Mathf.Abs(deltaSpeed) > 2.5f)
        {
            car.gasValue = 0.0f;
            car.turnValue = Random.Range(0, 2) * 2.0f - 1.0f;
            car.isWorking = false;

            evaporationTimer = EvaporationDelay + EvaporationTime;
        }
    }
}
