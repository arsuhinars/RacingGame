using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class GameUIManager : MonoBehaviour
{
    private const string GameViewName = "Game";
    private const string PauseViewName = "Pause";
    private const string GameEndViewName = "GameEnd";

    private const string RestartDialogTitleLocale = "restartDialogTitle";
    private const string RestartDialogDescriptionLocale = "restartDialogDescription";
    
    private const string ExitDialogTitleLocale = "exitDialogTitle";
    private const string ExitDialogDescriptionLocale = "exitDialogDescription";

    private const string YesLocale = "yes";
    private const string NoLocale = "no";
    private const string ExitLocale = "exit";

    // Ссылка на представителя класса одиночки
    public static GameUIManager Instance { get; private set; }

    [SerializeField] private UIGameCounter gameCounterPrefab;
    [SerializeField] private Transform gameCountersRoot;
    [SerializeField] private Transform gameModeHUDRoot;
    private readonly Dictionary<string, UIGameCounter> gameCounters = new();

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
        GameManager.Instance.OnGameStart += () =>
        {
            UIDialog.Default.Hide();
            UIManager.Instance.ChangeView(GameViewName);
        };
        GameManager.Instance.OnGameEnd += (string reason) =>
        {
            GameManager.Instance.UpdateGlobalVariables();
            UIManager.Instance.ChangeView(GameEndViewName);
        };
        GameManager.Instance.OnPause += () =>
        {
            GameManager.Instance.UpdateGlobalVariables();
            UIManager.Instance.ChangeView(PauseViewName);
        };
        GameManager.Instance.OnUnpause += () =>
        {
            UIDialog.Default.Hide();
            UIManager.Instance.ChangeView(GameViewName);
        };
        GameManager.Instance.OnGameCounterUpdate += (name, value) => 
        {
            if (gameCounters.TryGetValue(name, out var gameCounter))
                gameCounter.valueVariable.Value = value;
        };
        GameManager.Instance.OnLoaded += () =>
        {
            // Добавляем все счетчики текущего режима
            foreach (var counterName in GameManager.Instance.GameCountersNames)
            {
                var gameCounter = GameManager.Instance.GetCounterData(counterName);
                if (gameCounter.isHiddenInUI)
                    continue;

                var gameCounterElement = Instantiate(gameCounterPrefab, gameCountersRoot);
                gameCounterElement.nameText.text = LocalizationHelper.GetLocalizedString(counterName);

                gameCounterElement.valueVariable = new FloatVariable();
                gameCounter.valueText.Remove(GameModeCounter.ValueVariableName);
                gameCounter.valueText[GameModeCounter.ValueVariableName] = gameCounterElement.valueVariable;
                gameCounterElement.valueTextLocalizer.StringReference = gameCounter.valueText;

                gameCounters[counterName] = gameCounterElement;
            }

            // Создаем HUD игрового режима
            var gmHUD = GameManager.Instance.GameModeConfig.gameModeHUD;
            if (gmHUD != null)
            {
                Instantiate(gmHUD, gameModeHUDRoot);
            }
        };
    }

    public void OpenRestartDialog()
    {
        UIDialog.Default.Open(
            LocalizationHelper.GetLocalizedString(RestartDialogTitleLocale),
            LocalizationHelper.GetLocalizedString(RestartDialogDescriptionLocale),
            UIDialog.DefaultExitButton.Positive,
            LocalizationHelper.GetLocalizedString(NoLocale),
            null,
            LocalizationHelper.GetLocalizedString(YesLocale),
            GameManager.Instance.RestartGame
        );
    }

    public void OpenExitDialog()
    {
        UIDialog.Default.Open(
            LocalizationHelper.GetLocalizedString(ExitDialogTitleLocale),
            LocalizationHelper.GetLocalizedString(ExitDialogDescriptionLocale),
            UIDialog.DefaultExitButton.Positive,
            LocalizationHelper.GetLocalizedString(NoLocale),
            null,
            LocalizationHelper.GetLocalizedString(ExitLocale),
            GameManager.Instance.ReturnToMenu
        );
    }
}
