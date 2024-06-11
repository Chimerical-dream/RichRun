using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomMath
{
    public static float Clamp0360(float eulerAngles)
    {
        float result = eulerAngles - Mathf.CeilToInt(eulerAngles / 360f) * 360f;
        if (result < 0)
        {
            result += 360f;
        }
        return result;
    }

    public static float ClampPlusMinus1(float v)
    {
        return Mathf.Clamp(v, -1, 1);
    }

    public static int PlusMinusOne => UnityEngine.Random.Range(0f, 1f) > 0.5f ? -1 : 1;

    public static Vector2 Random(Vector2 a, Vector2 b)
    {
        return new Vector2(UnityEngine.Random.Range(a.x, b.x), UnityEngine.Random.Range(a.y, b.y));
    }

    public static Color SetHsvV(Color c, float v)
    {
        float h, s, x;

        Color.RGBToHSV(c, out h, out s, out x);
        return Color.HSVToRGB(h, s, v);
    }

    public static Color GetEmissionColor(Color c, float intensity)
    {
        float factor = Mathf.Pow(2, intensity);
        c = new Color(c.r * factor, c.g * factor, c.b * factor);
        return c;
    }
}
