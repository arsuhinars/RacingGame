using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameLoader : MonoBehaviour
{
    public static GameLoader Instance { get; private set; }

    private const string GameAssetsPrefabName = "GameAssets";

    public event Action OnReady;
    public event Action OnFailed;

    public bool IsDone { get; private set; } = false;

    public LevelData currentLevel;

    private AsyncOperationHandle<GameObject> gameAssetsHandle;
    private AsyncOperationHandle<GameObject> mapAssetsHandle;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;
    }

    private void Start()
    {
        if (currentLevel != null)
        {
            gameAssetsHandle = Addressables.LoadAssetAsync<GameObject>(GameAssetsPrefabName);
            mapAssetsHandle = Addressables.LoadAssetAsync<GameObject>(currentLevel.GetMapAssetName());
        }
    }

    private void Update()
    {
        if (IsDone || !gameAssetsHandle.IsValid() || !mapAssetsHandle.IsValid())
            return;

        if (gameAssetsHandle.IsDone && mapAssetsHandle.IsDone)
        {
            IsDone = true;
            if (gameAssetsHandle.Status == AsyncOperationStatus.Failed ||
                mapAssetsHandle.Status == AsyncOperationStatus.Failed)
            {
                StartCoroutine(HandleOnDone(false));
            }
            else
            {
                Instantiate(gameAssetsHandle.Result);
                Instantiate(mapAssetsHandle.Result);
                StartCoroutine(HandleOnDone(true));
            }
        }
    }

    private IEnumerator HandleOnDone(bool success)
    {
        yield return null;
        if (success)
            OnReady?.Invoke();
        else
            OnFailed?.Invoke();

        Addressables.Release(gameAssetsHandle);
        Addressables.Release(mapAssetsHandle);

        Destroy(gameObject);
    }
}
