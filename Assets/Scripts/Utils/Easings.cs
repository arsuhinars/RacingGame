using UnityEngine;

// Класс с функциями перехода
public static class Easings
{
    // Переход по синусоиде
    public static float EaseInSine(float t) => 1.0f - Mathf.Cos((t * Mathf.PI) * 0.5f);
    public static float EaseOutSine(float t) => Mathf.Sin((t * Mathf.PI) * 0.5f);
    public static float EaseInOutSine(float t) => -(Mathf.Cos(t * Mathf.PI) - 1.0f) * 0.5f;

    // Переход по квадратичной параболе
    public static float EaseInQuad(float t) => t * t;
    public static float EaseOutQuad(float t) => 1.0f - (1.0f - t) * (1.0f - t);
    public static float EaseInOutQuad(float t) => t < 0.5f ? 2.0f * t * t : 1 - Mathf.Pow(-2.0f * t + 2.0f, 2.0f) * 0.5f;

    // Переход по кубической параболе
    public static float EaseInCubic(float t) => t * t * t;
    public static float EaseOutCubic(float t) => 1.0f - Mathf.Pow(1.0f - t, 3.0f);
    public static float EaseInOutCubic(float t) => t < 0.5f ? 4 * t * t * t : 1.0f - Mathf.Pow(-2.0f * t + 2, 3.0f) * 0.5f;

    // Переход по параболе четвертой степени
    public static float EaseInQuart(float t) => t * t * t * t;
    public static float EaseOutQuart(float t) => 1.0f - Mathf.Pow(1 - t, 4.0f);
    public static float EaseInOutQuart(float t)
    {
        return t < 0.5f ?
            8.0f * t * t * t * t :
            1.0f - Mathf.Pow(-2.0f * t + 2.0f, 4.0f) * 0.5f;
    }
}
