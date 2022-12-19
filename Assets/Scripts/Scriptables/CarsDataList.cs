using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class CarData
{
    public CarConfig CarConfig
    {
        get
        {
            if (!carConfigHandle.IsValid())
                PreloadCarConfig();
            if (!carConfigHandle.IsDone)
                carConfigHandle.WaitForCompletion();
            return carConfig;
        }
    }

    public AssetReference carConfigReference;
    [Tooltip("Set to 0 if you want to make it available from start")]
    public int price = 0;

    private AsyncOperationHandle<GameObject> carConfigHandle;
    private CarConfig carConfig;

    public void PreloadCarConfig()
    {
        if (carConfigHandle.IsValid())
            return;

        carConfigHandle = carConfigReference.LoadAssetAsync<GameObject>();
        carConfigHandle.Completed += (handle) => carConfig = handle.Result.GetComponent<CarConfig>();
    }

    public void ReleaseCarConfig()
    {
        carConfigReference.ReleaseAsset();
    }
}

[CreateAssetMenu(menuName = "Game/Cars Data List", fileName = "CarsDataList")]
public class CarsDataList : ScriptableObject
{
    private const string AssetName = "CarsDataList";

    public static CarsDataList Instance
    {
        get 
        {
            if (_instance == null)
            {
                var handle = Addressables.LoadAssetAsync<CarsDataList>(AssetName);
                handle.WaitForCompletion();
                _instance = handle.Result;
            }
            return _instance;
        }
    }
    private static CarsDataList _instance;

    public CarData[] list;
    private Dictionary<string, int> carsIdsByNames;

    public static void PreloadCars()
    {
        foreach (var carData in Instance.list)
        {
            carData.PreloadCarConfig();
        }
    }

    public static void ReleaseCars()
    {
        foreach (var carData in Instance.list)
        {
            carData.ReleaseCarConfig();
        }
    }

    public static CarData GetCarDataById(int id)
    {
        return Instance.list[id];
    }
}
