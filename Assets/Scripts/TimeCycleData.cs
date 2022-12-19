using System;
using UnityEngine;

[Serializable]
public struct TimeCycleData
{
    public Vector3 sunDirection;
    public Color sunColor;
    public Color ambientColor;
    [Space]
    public Color fogColor;
    public float fogDensity;
    [Space]
    public Color foliageColor;
    [Space]
    public float puddlesFactor;
    public float snowFactor;
    [Space]
    public float carAngleSmoothTimeFactor;

    public static TimeCycleData Lerp(TimeCycleData a, TimeCycleData b, float t)
    {
        return new TimeCycleData()
        {
            sunDirection = Vector3.SlerpUnclamped(a.sunDirection, b.sunDirection, t),
            sunColor = Color.LerpUnclamped(a.sunColor, b.sunColor, t),
            ambientColor = Color.LerpUnclamped(a.ambientColor, b.ambientColor, t),
            fogColor = Color.LerpUnclamped(a.fogColor, b.fogColor, t),
            fogDensity = Mathf.LerpUnclamped(a.fogDensity, b.fogDensity, t),
            foliageColor = Color.LerpUnclamped(a.foliageColor, b.foliageColor, t),
            puddlesFactor = Mathf.LerpUnclamped(a.puddlesFactor, b.puddlesFactor, t),
            snowFactor = Mathf.LerpUnclamped(a.snowFactor, b.snowFactor, t),
            carAngleSmoothTimeFactor = Mathf.LerpUnclamped(a.carAngleSmoothTimeFactor, b.carAngleSmoothTimeFactor, t)
        };
    }
}
