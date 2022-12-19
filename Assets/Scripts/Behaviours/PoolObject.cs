using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

[SelectionBase]
public class PoolObject : MonoBehaviour
{
    [HideInInspector] public bool isActiveInPool = false;
    [HideInInspector] public IObjectPool<PoolObject> pool;

    [SerializeField] private UnityEvent _onSpawn;
    [SerializeField] private UnityEvent _onRelease;

    public UnityEvent OnSpawn => _onSpawn;
    public UnityEvent OnRelease => _onRelease;

    public ParticleSystem ParticleSystem { get; private set; }
    public MeshRenderer MeshRenderer { get; private set; }
    public CarController CarController { get; private set; }
    
    // Параметры в инспекторе
    [SerializeField] private bool releaseWhenDisabled = true;

    private void Awake()
    {
        ParticleSystem = GetComponent<ParticleSystem>();
        CarController = GetComponent<CarController>();
        MeshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    private void OnDisable()
    {
        if (releaseWhenDisabled && isActiveInPool)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        pool?.Release(this);
    }

    public void HandleSpawn() => OnSpawn?.Invoke();

    public void HandleRelease() => OnRelease?.Invoke();
}
