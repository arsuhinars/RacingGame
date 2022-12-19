using System;
using System.Collections.Generic;
using UnityEngine;
using GameObjectPool = UnityEngine.Pool.ObjectPool<PoolObject>;

[AddComponentMenu("Managers/Pool Manager")]
public class PoolsManager : MonoBehaviour
{
    [Serializable]
    private class PoolConfig
    {
        public string name;
        public PoolObject prefab;
        public int defaultCapacity = 10;
        public int maxSize = 10000;
        public bool toggleGameObjectState = true;
        public Transform root;

        [NonSerialized] public GameObjectPool pool;
    }

    // Ссылка на представителя класса одиночки
    public static PoolsManager Instance { get; private set; }

    // Параметры в инспекторе
    [SerializeField] private PoolConfig[] poolsConfigs;

    private readonly Dictionary<string, GameObjectPool> pools = new();

    private void Awake()
    {
        // Делаем класс одиночкой
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;

        // Создаем и добавляем в словарь pools все пулы по их конфигам
        foreach (var poolCfg in poolsConfigs)
        {
            try
            {
                var poolConfig = poolCfg;
                pools.Add(poolConfig.name, new GameObjectPool(
                    () =>
                    {
                        var obj = Instantiate(poolConfig.prefab, poolConfig.root);
                        obj.pool = poolConfig.pool;
                        return obj;
                    },
                    (obj) =>
                    {
                        obj.isActiveInPool = true;
                        if (poolConfig.toggleGameObjectState)
                            obj.gameObject.SetActive(true);
                        obj.HandleSpawn();
                    },
                    (obj) =>
                    {
                        obj.isActiveInPool = false;
                        obj.transform.SetParent(poolConfig.root);
                        obj.HandleRelease();
                        if (poolConfig.toggleGameObjectState)
                            obj.gameObject.SetActive(false);
                    },
                    (obj) => Destroy(obj),
                    true, poolConfig.defaultCapacity, poolConfig.maxSize
                ));
                poolConfig.pool = pools[poolConfig.name];

                if (poolConfig.root == null)
                {
                    poolConfig.root = new GameObject(poolConfig.name).transform;
                    poolConfig.root.SetParent(transform);
                }
            }
            catch (ArgumentException)
            {
                // Название пула должно быть уникальным
                Debug.LogError("Pool's name must be unique!", this);
            }
        }
    }

    // Получаем нужный пул по имени
    public GameObjectPool GetPool(string name)
    {
        try
        {
            return pools[name];
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
}
