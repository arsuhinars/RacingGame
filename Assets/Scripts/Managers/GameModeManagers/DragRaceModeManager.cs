using UnityEngine;

public class DragRaceModeManager : MonoBehaviour
{
    public const string RemainedDistanceCounterName = "remainedDistance";

    private const string DistanceParameterName = "distance";

    public static DragRaceModeManager Instance { get; private set; }

    public float RemainedDistance
    {
        get => GameManager.Instance.GetCounterValue(RemainedDistanceCounterName);
        private set =>
            GameManager.Instance.SetCounterValue(RemainedDistanceCounterName, value);
    }
    public float TargetDistance { get; private set; }

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
        GameManager.Instance.OnGameStart += () =>
        {
            TargetDistance = GameManager.Instance.LevelData.gameModeParameters[DistanceParameterName];
            RemainedDistance = TargetDistance;
        };
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameStarted)
            return;

        RemainedDistance -= MapManager.Instance.MoveSpeed * Time.deltaTime;
        if (RemainedDistance <= 0f)
        {
            GameManager.Instance.EndGame(GameManager.EndReasonRaceEnded);
        }
    }
}
