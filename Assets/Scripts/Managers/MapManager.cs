using System;
using UnityEngine;
using Random = UnityEngine.Random;

[AddComponentMenu("Managers/Map  Manager")]
public class MapManager : MonoBehaviour
{
    // Ссылка на представителя класса одиночки
    public static MapManager Instance { get; private set; }

    [Serializable]
    private class MapPart
    {
        public string startSectionPool;
        public string middleSectionPool;
        public string endSectionPool;

        public int minLength = 1;
        public int maxLength = 1;
    }

    public float SectionsSpawnDistance => _sectionsSpawnDistance;
    public float SectionsReleaseDistance => _sectionsReleaseDistance;

    // Параметры в инспекторе
    [SerializeField] private MapPart[] mapParts;
    [SerializeField] private float _sectionsSpawnDistance;
    [SerializeField] private float _sectionsReleaseDistance;

    public event Utils.ValueChange<float> OnInstantSpeedChange;
    public event Action<MapSection> OnSectionSpawn;

    public float MoveAcceleration { get; set; }
    public float MoveSpeed
    {
        get => _moveSpeed;
        set
        {
            float old = _moveSpeed;
            _moveSpeed = value;
            OnInstantSpeedChange?.Invoke(value, old);
        }
    }

    private float _moveSpeed = 0.0f;
    private int lastPartIndex;
    private MapPart currPart;
    private int levelPartLength;
    private int sectionsCounter;
    private MapSection lastSection;

    public MapSection GetLastSection() => lastSection;

    private void Awake()
    {
        // Делаем класс одиночкой
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;
    }

    private void Start()
    {
        lastPartIndex = Random.Range(0, mapParts.Length);

        GameManager.Instance.OnGameStart += () => currPart = null;
    }

    private void LateUpdate()
    {
        if (!GameManager.Instance.IsLoaded)
            return;

        while (lastSection == null || !lastSection.isActiveAndEnabled || lastSection.offset + lastSection.Length * 0.5f <= SectionsSpawnDistance)
        {
            SpawnSection();
        }
    }

    private void FixedUpdate()
    {
        _moveSpeed += MoveAcceleration * Time.deltaTime;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        const float width = 100.0f;

        var v1 = 0.5f * width * Vector3.right + Vector3.forward * SectionsReleaseDistance;
        var v2 = v1 + Vector3.forward * (SectionsSpawnDistance - SectionsReleaseDistance);
        var v3 = v2 - Vector3.right * width;
        var v4 = v1 - Vector3.right * width;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(v1, v2);
        Gizmos.DrawLine(v2, v3);
        Gizmos.DrawLine(v3, v4);
        Gizmos.DrawLine(v4, v1);
    }
#endif

    private MapSection SpawnSection()
    {
        if (currPart == null)
        {
            int partIndex = Random.Range(0, mapParts.Length - 1);
            if (partIndex >= lastPartIndex)
                partIndex++;
            
            currPart = mapParts[partIndex];
            lastPartIndex = partIndex;

            levelPartLength = Random.Range(currPart.minLength, currPart.maxLength + 1);
            sectionsCounter = 0;
        }

        var sectionPrefab = currPart.middleSectionPool;
        if (sectionsCounter == 0 && currPart.startSectionPool.Length > 0)
            sectionPrefab = currPart.startSectionPool;
        else if (sectionsCounter == levelPartLength - 1 && currPart.endSectionPool.Length > 0)
            sectionPrefab = currPart.endSectionPool;

        if (sectionsCounter == levelPartLength - 1)
            currPart = null;

        sectionsCounter++;

        bool isLastSectionActive = lastSection != null && lastSection.isActiveAndEnabled;

        var sectionObj = PoolsManager.Instance.GetPool(sectionPrefab).Get();
        if (sectionObj == null)
            return null;

        var newSection = sectionObj.GetComponent<MapSection>();
        if (newSection == null)
        {
            sectionObj.ReturnToPool();
            Debug.LogError("Section prefab should contain LevelSection component!", this);
        }

        if (isLastSectionActive)
            newSection.offset = lastSection.offset + lastSection.Length * 0.5f;
        else
            newSection.offset = SectionsReleaseDistance;
        newSection.offset += newSection.Length * 0.5f;

        newSection.UpdatePosition();

        lastSection = newSection;

        OnSectionSpawn?.Invoke(newSection);

        return newSection;
    }
}
