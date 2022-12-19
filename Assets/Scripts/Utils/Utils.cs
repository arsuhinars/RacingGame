using System;
using UnityEngine;

public class Utils
{
    public delegate void ValueChange<T>(T value, T old);

    public static Quaternion QuaternionFromVector4(Vector4 vector)
    {
        return new Quaternion(vector.x, vector.y, vector.z, vector.w);
    }

    public static Vector4 QuaternionToVector4(Quaternion rotation)
    {
        return new Vector4(rotation.x, rotation.y, rotation.z, rotation.w);
    }

    public static T GetEnumFromPlayerPrefs<T>(string key, T defaultValue) where T : struct
    {
        var value = PlayerPrefs.GetString(key);
        if (Enum.TryParse<T>(value, out var result))
            return result;
        else
            return defaultValue;
    }

    public static void SetEnumInPlayerPrefs<T>(string key, T value)
    {
        PlayerPrefs.SetString(key, value.ToString());
    }
}
