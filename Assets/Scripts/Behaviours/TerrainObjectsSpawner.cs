using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainObjectsSpawner : MonoBehaviour
{
    [Serializable]
    private class PrefabData
    {
        public string poolName;
        public float maxYOffset;
        public float minScale = 1.0f;
        public float maxScale = 1.0f;
    }

    public float density = 1.0f;
    public Vector2 zoneSize;
    [SerializeField] private ChanceValueElector<PrefabData> prefabsChoicer;

    private readonly List<PoolObject> terrainObjects = new();

    public void SpawnObjects()
    {
        ReleaseObjects();

        int xCount = (int)(zoneSize.x / density);
        int zCount = (int)(zoneSize.y / density);

        for (int x = 0; x < xCount; x++)
        {
            for (int z = 0; z < zCount; z++)
            {
                var prefabData = prefabsChoicer.GetNextValueByChance();
                if (prefabData == null)
                    continue;

                var pool = PoolsManager.Instance.GetPool(prefabData.poolName);
                if (pool == null)
                    continue;

                var posOffset = new Vector3(
                    x * density + Random.Range(0.0f, density),
                    Random.Range(-prefabData.maxYOffset, 0.0f),
                    z * density + Random.Range(0.0f, density)
                );

                var obj = pool.Get();
                obj.transform.SetPositionAndRotation(
                    transform.position + posOffset,
                    Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f)
                );
                obj.transform.localScale = Vector3.one * Random.Range(prefabData.minScale, prefabData.maxScale);
                obj.transform.SetParent(transform);
                terrainObjects.Add(obj);
            }
        }
    }

    public void ReleaseObjects()
    {
        foreach (var poolObj in terrainObjects)
        {
            poolObj.ReturnToPool();
        }
        terrainObjects.Clear();
    }
}
