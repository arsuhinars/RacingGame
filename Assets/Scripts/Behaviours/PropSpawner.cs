using UnityEngine;

public class PropSpawner : MonoBehaviour, ISpawnable
{
    [SerializeField] private ChanceValueElector<PropRandomizer> props;

    private PoolObject activeObject;

    public void Spawn()
    {
        Despawn();
        activeObject = props.GetNextValueByChance().Get(Vector3.zero, Quaternion.identity, transform);
    }

    public void Despawn()
    {
        if (activeObject != null)
        {
            activeObject.ReturnToPool();
            activeObject = null;
        }
    }
}