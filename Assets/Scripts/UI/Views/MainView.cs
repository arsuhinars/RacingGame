using TMPro;
using UnityEngine;

[RequireComponent(typeof(UIView))]
public class MainView : MonoBehaviour
{
    [SerializeField] private Transform carRoot;
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI carNameText;
    [SerializeField] private TextMeshProUGUI classCaptionText;
    [SerializeField] private SerializableDictionary<CarConfig.CarClass, GameObject> carClassBackgrounds;
    [SerializeField] private ProgressBar carSpeed;
    [SerializeField] private ProgressBar carAcceleration;
    [SerializeField] private ProgressBar carHandling;

    private GameObject carObject;
    private UIView view;

    private void Start()
    {
        SetCurrentCar(PlayerProgressManager.PlayerCarId);
    }

    public void SetCurrentCar(int carId)
    {
        PlayerProgressManager.PlayerCarId = carId;

        Destroy(carObject);

        var carConfig = CarsDataList.GetCarDataById(carId).CarConfig;
        carObject = Instantiate(carConfig, carRoot).gameObject;

        carNameText.text = carConfig.GetLocalizedName();
        classCaptionText.text = CarConfig.GetCarClassName(carConfig.classType);
        foreach (var kp in carClassBackgrounds)
            kp.Value.SetActive(carConfig.classType == kp.Key);
    }

    public void NextCar()
    {
        int carId = PlayerProgressManager.PlayerCarId;
        if (++carId >= CarsDataList.Instance.list.Length)
        {
            carId = 0;
        }
        SetCurrentCar(carId);
    }
    
    public void PreviousCar()
    {
        int carId = PlayerProgressManager.PlayerCarId;
        if (--carId < 0)
        {
            carId = CarsDataList.Instance.list.Length - 1;
        }
        SetCurrentCar(carId);
    }
}
