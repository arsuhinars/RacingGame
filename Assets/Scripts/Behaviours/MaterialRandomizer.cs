using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MaterialRandomizer : MonoBehaviour
{
    [SerializeField] private Material[] materials;

    private MeshRenderer mesh;

    private void OnEnable()
    {
        RandomizeMaterial();
    }

    public void RandomizeMaterial()
    {
        if (materials.Length == 0)
            return;

        if (mesh == null)
            mesh = GetComponent<MeshRenderer>();

        mesh.sharedMaterial = materials[Random.Range(0, materials.Length)];
    }
}
