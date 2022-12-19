using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public static class PlayerProgressManager
{
    private const string MoneyPrefsKey = "Money";
    private const string MoneyVariableName = "money";
    private const string ReputationLevelPrefsKey = "ReputationLevel";
    private const string ReputationLevelVariableName = "reputationLevel";
    private const string ReputationScorePrefsKey = "ReputationScores";
    private const string ReputationScoreVariableName = "reputationScores";
    private const string SparesPrefsKey = "Spares";
    private const string SparesVariableName = "spares";
    private const string PlayerCarIdKey = "PlayerCarId";

    public static int Money
    {
        get => PlayerPrefs.GetInt(MoneyPrefsKey, 0);
        set
        {
            PlayerPrefs.SetInt(MoneyPrefsKey, value);
            moneyVar.Value = value;
        }
    }
    public static int ReputationLevel
    {
        get => PlayerPrefs.GetInt(ReputationLevelPrefsKey, 1);
        set
        {
            PlayerPrefs.SetInt(ReputationLevelPrefsKey, value);
            reputationLevelVar.Value = value;
        }
    }
    public static int ReputationScore
    {
        get => PlayerPrefs.GetInt(ReputationScorePrefsKey, 0);
        set
        {
            PlayerPrefs.SetInt(ReputationScorePrefsKey, value);
            reputationScoreVar.Value = value;
        }
    }
    public static int Spares
    {
        get => PlayerPrefs.GetInt(SparesPrefsKey, 0);
        set
        {
            PlayerPrefs.SetInt(SparesPrefsKey, value);
            sparesVar.Value = value;
        }
    }
    public static int PlayerCarId
    {
        get => PlayerPrefs.GetInt(PlayerCarIdKey, 0);
        set => PlayerPrefs.SetInt(PlayerCarIdKey, value);
    }

    private readonly static IntVariable moneyVar;
    private readonly static IntVariable reputationLevelVar;
    private readonly static IntVariable reputationScoreVar;
    private readonly static IntVariable sparesVar;

    static PlayerProgressManager()
    {
        moneyVar = LocalizationHelper.GetGlobalVariable<IntVariable>(MoneyVariableName);
        reputationLevelVar = LocalizationHelper.GetGlobalVariable<IntVariable>(ReputationLevelVariableName);
        reputationScoreVar = LocalizationHelper.GetGlobalVariable<IntVariable>(ReputationScoreVariableName);
        sparesVar = LocalizationHelper.GetGlobalVariable<IntVariable>(SparesVariableName);

        UpdateGlobalVariables();
    }

    public static void UpdateGlobalVariables()
    {
        moneyVar.Value = Money;
        reputationLevelVar.Value = ReputationLevel;
        reputationScoreVar.Value = ReputationScore;
        sparesVar.Value = Spares;
    }
}
