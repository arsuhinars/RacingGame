using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class GameModeCounter
{
    public const string ValueVariableName = "value";

    public string name;
    public LocalizedString valueText;
    public bool isHiddenInUI = false;
    public bool isHiddenInPause = false;
}

[CreateAssetMenu(menuName = "Game/Game mode config", fileName = "GameModeConfig")]
public class GameModeConfig : ScriptableObject
{
    public enum RecordType
    {
        Largest, Smallest
    }

    public string gameModeName;
    public Sprite gameModeIcon;
    public Component gameModeManager;
    public GameObject gameModeHUD;
    public GameModeCounter[] gameCounters;
    [Space]
    public string recordCounterName;
    public RecordType recordType;

    public GameModeCounter GetCounterByName(string name)
    {
        foreach (var counter in gameCounters)
        {
            if (counter.name == name)
                return counter;
        }
        return null;
    }
}
