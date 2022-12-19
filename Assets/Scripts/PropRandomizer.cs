using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PropRandomizer
{
    public enum RotateType
    {
        NoRotate, FreeRotate, RotateBy90Degree
    }

    public string poolName;
    public RotateType rotateType;
    public float maxYOffset = 0f;
    public float minScale = 1f;
    public float maxScale = 1f;
    [Header("Material randomizer")]
    [Tooltip("Left empty if it is not needed to change materials")]
    public ChanceValueElector<Material> materials;
    public int materialIndex;

    public PoolObject Get(Vector3 offset, Quaternion rotation, Transform parent)
    {
        var pool = PoolsManager.Instance.GetPool(poolName);
        if (pool == null)
            return null;

        var activeObject = pool.Get();
        activeObject.transform.SetParent(parent);
        activeObject.transform.localPosition = offset + Vector3.down * Random.Range(0f, maxYOffset);
        activeObject.transform.localRotation = rotation * Quaternion.Euler(0f, rotateType switch
        {
            RotateType.FreeRotate => Random.Range(0f, 360f),
            RotateType.RotateBy90Degree => Random.Range(0, 4) * 90f,
            _ => 0f
        }, 0f);
        activeObject.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);

        if (materials.Count > 0)
        {
            activeObject.MeshRenderer.sharedMaterials[materialIndex] = materials.GetNextValueByChance();
        }

        return activeObject;
    }
}