using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlendMethod
{
    public enum BlendMode
	{
        Normal,
        Add,
        Sub,
        Mul,
        Div,
        Min,
        Max
	}

    private static float NormalBlend(float bot, float top, float opacity)
	{
        return Mathf.Lerp(bot, top, opacity);
	}
    private static float AddBlend(float bot, float top, float opacity)
	{
        return Mathf.Lerp(bot, bot + top, opacity);
	}
    private static float SubBlend(float bot, float top, float opacity)
	{
        return Mathf.Lerp(bot, top - bot, opacity);
	}
    private static float MulBlend(float bot, float top, float opacity)
	{
        return Mathf.Lerp(bot, bot * top, opacity);
	}
    private static float DivBlend(float bot, float top, float opacity)
	{
        return Mathf.Lerp(bot, top / bot, opacity);
	}
    private static float MinBlend(float bot, float top, float opacity)
	{
        return Mathf.Lerp(bot, Mathf.Min(bot, top), opacity);
	}
    private static float MaxBlend(float bot, float top, float opacity)
	{
        return Mathf.Lerp(bot, Mathf.Max(bot, top), opacity);
	}

    public static float Blend(float bot, float top, float opacity, BlendMode blendMode)
	{
        return blendMode switch
        {
            BlendMode.Normal => NormalBlend(bot, top, opacity),
            BlendMode.Add => AddBlend(bot, top, opacity),
            BlendMode.Sub => SubBlend(bot, top, opacity),
            BlendMode.Mul => MulBlend(bot, top, opacity),
            BlendMode.Div => DivBlend(bot, top, opacity),
            BlendMode.Min => MinBlend(bot, top, opacity),
            BlendMode.Max => MaxBlend(bot, top, opacity),
            _ => 0.0f //neverHappen
        };
	}
}
