using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public const string MainViewName = "Main";
    public const string PlayViewName = "Play";
    public const string LevelViewName = "Level";

    private const string GameSceneName = "GameScene";

    public static MainMenuManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;

        PlayerProgressManager.UpdateGlobalVariables();
    }

    private IEnumerator Start()
    {
        Time.timeScale = 1f;

        yield return Addressables.InitializeAsync();
        CarsDataList.PreloadCars();

        yield return null;
        SettingsManager.Instance.RefreshSettings();
        UIManager.Instance.ChangeView(MainViewName);
    }

    private void OnDestroy()
    {
        CarsDataList.ReleaseCars();
    }

    public void StartLevel(LevelData level)
    {
        SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);

        var gameLoader = new GameObject();
        gameLoader.AddComponent<GameLoader>().currentLevel = level;

        DontDestroyOnLoad(gameLoader);
    }
}
