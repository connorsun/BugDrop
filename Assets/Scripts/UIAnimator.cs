using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class UIAnimator
{
    // -- EASING TYPES --
    public enum EasingType
    {
        EaseOutCubic,
        EaseInCubic,
        EaseOutOvershoot
    }

    // -- EASING CURVES --
    public static float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    public static float EaseInCubic(float t)
    {
        return t * t * t;
    }

    public static float EaseOutOvershoot(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // -- ANIMATIONS --
    public static async Task FadeTo(CanvasGroup element, float target, float duration, EasingType easeType)
    {
        Func<float, float> ease = EasingFunction(easeType);
        float start = element.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            element.alpha = Mathf.LerpUnclamped(start, target, ease(Mathf.Clamp01(elapsed / duration)));
            await Task.Yield();
        }
        element.alpha = target;
    }

    public static async Task SlideTo(RectTransform element, Vector2 target, float duration, EasingType easeType)
    {
        Func<float, float> ease = EasingFunction(easeType);
        Vector2 start = element.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            element.anchoredPosition = Vector2.LerpUnclamped(start, target, ease(Mathf.Clamp01(elapsed / duration)));
            await Task.Yield();
        }
        element.anchoredPosition = target;
    }

    public static async Task ScaleTo(Transform element, Vector3 target, float duration, EasingType easeType)
    {
        Func<float, float> ease = EasingFunction(easeType);
        Vector3 start = element.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            element.localScale = Vector3.LerpUnclamped(start, target, ease(Mathf.Clamp01(elapsed / duration)));
            await Task.Yield();
        }
        element.localScale = target;
    }

    private static Func<float, float> EasingFunction(EasingType easing)
    {
        switch(easing)
        {
            case EasingType.EaseOutCubic:
                return EaseOutCubic;
            case EasingType.EaseInCubic:
                return EaseInCubic;
            case EasingType.EaseOutOvershoot:
                return EaseOutOvershoot;
            default:
                return EaseOutCubic;
        }
    }
}
