using UnityEngine;

class TimeTrialModeManager : MonoBehaviour
{
    public const string RemainedTimeCounterName = "remainedTime";

    private const string RemainedTimeParameterName = "time";

    public float RemainedTime 
    { 
        get => GameManager.Instance.GetCounterValue(RemainedTimeCounterName);
        private set
        {
            GameManager.Instance.SetCounterValue(RemainedTimeCounterName, value);
        }
    }

    private void Start()
    {
        GameManager.Instance.OnGameStart += () =>
        {
            RemainedTime = GameManager.Instance.LevelData.gameModeParameters[RemainedTimeParameterName];
        };
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameStarted)
            return;

        RemainedTime -= Time.deltaTime;
        if (RemainedTime <= 0f)
        {
            GameManager.Instance.EndGame(GameManager.EndReasonTimeIsOver);
        }
    }
}
