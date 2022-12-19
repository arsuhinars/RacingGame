using UnityEngine;

[RequireComponent(typeof(PoolObject))]
[RequireComponent(typeof(CarController))]
public class PoliceController : MonoBehaviour
{
    private CarController car;
    private PoolObject poolObject;

    private void Awake()
    {
        car = GetComponent<CarController>();
        poolObject = GetComponent<PoolObject>();
    }

    private void Start()
    {
        GameManager.Instance.OnGameStart += () =>
        {
            if (poolObject.isActiveInPool)
                poolObject.ReturnToPool();
        };
    }

    private void OnEnable()
    {
        car.ResetState();
        car.gasValue = 1f;
    }

    private void Update()
    {
        if (transform.position.z + TrafficManager.Instance.AverageCarSize * 0.5f < TrafficManager.Instance.MinTrafficDistance ||
            transform.position.z - TrafficManager.Instance.AverageCarSize * 0.5f > TrafficManager.Instance.MaxTrafficDistance)
        {
            poolObject.ReturnToPool();
        }
    }
}
