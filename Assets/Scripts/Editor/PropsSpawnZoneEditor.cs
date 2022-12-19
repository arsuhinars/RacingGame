using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PropsSpawnZone))]
[CanEditMultipleObjects]
public class PropsSpawnZoneEditor : Editor
{
    private PropsSpawnZone t;

    private void OnEnable()
    {
        t = (PropsSpawnZone)target;
    }

    private void OnSceneGUI()
    {
        var pos = t.transform.position;

        Handles.color = Color.red;
        for (float x = 0f; x < t.zoneSize.x; x += t.density)
        {
            Handles.DrawLine(
                pos + new Vector3(x, 0f, 0f),
                pos + new Vector3(x, 0f, t.zoneSize.y)
            );
        }

        for (float y = 0f; y < t.zoneSize.y; y += t.density)
        {
            Handles.DrawLine(
                pos + new Vector3(0f, 0f, y),
                pos + new Vector3(t.zoneSize.x, 0f, y)
            );
        }

        var points = new Vector3[]
        {
            pos,
            pos + new Vector3(0.0f, 0.0f, t.zoneSize.y),
            pos + new Vector3(t.zoneSize.x, 0.0f, t.zoneSize.y),
            pos + new Vector3(t.zoneSize.x, 0.0f, 0.0f),
            pos
        };
        Handles.DrawPolyLine(points);

        EditorGUI.BeginChangeCheck();

        Handles.color = Color.white;
        var v = Handles.FreeMoveHandle(points[2], Quaternion.identity, 0.5f, Vector3.one * 0.1f, Handles.RectangleHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(t, "Change spawn zone size");

            t.zoneSize.x = v.x - t.transform.position.x;
            t.zoneSize.y = v.z - t.transform.position.z;
        }
    }
}
