using System.Collections.Generic;
using UnityEngine;

public class PropsSpawnZone : MonoBehaviour, ISpawnable
{
    [Min(0.01f)] public float density = 1f;
    public Vector2 maxOffsetFactor = new(1f, 1f);
    public Vector2 zoneSize;
    [SerializeField] private ChanceValueElector<PropRandomizer> props;

    private readonly List<PoolObject> activeProps = new();

    public void Spawn()
    {
        Despawn();

        int xCount = Mathf.Max(1, Mathf.FloorToInt(zoneSize.x / density));
        int zCount = Mathf.Max(1, Mathf.FloorToInt(zoneSize.y / density));

        for (int x = 0; x < xCount; x++)
        {
            for (int z = 0; z < zCount; z++)
            {
                var prop = props.GetNextValueByChance().Get(
                    offset: new Vector3(
                        x * density + Random.Range(0.0f, density) * maxOffsetFactor.x,
                        0f,
                        z * density + Random.Range(0.0f, density) * maxOffsetFactor.y
                    ),
                    rotation: Quaternion.identity,
                    parent: transform
                );

                if (prop != null)
                {
                    activeProps.Add(prop);
                }
            }
        }
    }

    public void Despawn()
    {
        foreach (var prop in activeProps)
        {
            prop.ReturnToPool();
        }
        activeProps.Clear();
    }
}
