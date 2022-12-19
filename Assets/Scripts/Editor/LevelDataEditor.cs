using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private const string MapNameProperty = "mapName";
    private const string MapSeedProperty = "mapSeed";
    private const string GameModeNameProperty = "gameModeName";
    private const string TrafficDensityProperty = "trafficDensity";
    private const string GameModeParametersProperty = "gameModeParameters";
    private const string PlayerCarProperty = "playerCar";
    private const string PlayerCarColorProperty = "playerCarColor";
    private const string GenerateButtonContent = "Generate";
    private const string SetRandomButtonContent = "Random";

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(MapNameProperty));


        EditorGUILayout.PropertyField(serializedObject.FindProperty(GameModeNameProperty));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(GameModeParametersProperty));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(TrafficDensityProperty));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(PlayerCarProperty));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(PlayerCarColorProperty));

        serializedObject.ApplyModifiedProperties();
    }
}