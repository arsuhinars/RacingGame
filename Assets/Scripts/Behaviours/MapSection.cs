using System;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(PoolObject))]
public class MapSection : MonoBehaviour
{
    // Параметры в инспекторе
    [Tooltip("Небольшое указание: Origin секции должен находится в фактическом центре модели\n" +
        "для правильного вычисления её границ.")]
    [SerializeField] private float length;
    [SerializeField, RequireInterface(typeof(ISpawnable))] private UnityEngine.Object[] spawnables;

    public float Length => length;
    [NonSerialized] public float offset = 0.0f;

    private PoolObject poolObject;

    private void Start()
    {
        poolObject = GetComponent<PoolObject>();

        GameManager.Instance.OnGameStart += () =>
        {
            if (poolObject.isActiveInPool)
                poolObject.ReturnToPool();
        };
    }

    private void OnEnable()
    {
        if (spawnables == null)
            return;

        foreach (var spawner in spawnables)
        {
            (spawner as ISpawnable).Spawn();
        }
    }

    private void OnDisable()
    {
        if (spawnables == null)
            return;

        foreach (var spawner in spawnables)
        {
            (spawner as ISpawnable).Despawn();
        }
    }

    private void Update()
    {
        offset -= Time.deltaTime * MapManager.Instance.MoveSpeed;

        UpdatePosition();

        if (offset + length * 0.5f < MapManager.Instance.SectionsReleaseDistance)
        {
            poolObject.ReturnToPool();
        }
    }

    public void UpdatePosition()
    {
        var currPos = transform.localPosition;
        currPos.z = offset;

        transform.localPosition = currPos;
    }
}