using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UIAnimatable : MonoBehaviour
{
    // --- TYPES ---
    public enum AnimationType
    {
        Fade,
        Slide,
        FadeAndSlide,
        Scale
    }
    
    // -- TRANSITION SPECS --

    // Show
    [Header("Show")]
    [SerializeField] private AnimationType showAnimation = AnimationType.Fade;
    [SerializeField] private UIAnimator.EasingType showEasing = UIAnimator.EasingType.EaseOutCubic;
    [SerializeField] private float showDuration = 0.25f;
    

    // Hide
    [Header("Hide")]
    [SerializeField] private AnimationType hideAnimation = AnimationType.Fade;
    [SerializeField] private UIAnimator.EasingType hideEasing = UIAnimator.EasingType.EaseInCubic;
    [SerializeField] private float hideDuration = 0.25f;
    

    // Slide & Scale
    [Header("Slide & Scale")]
    [SerializeField] private Vector2 slideOffset = new Vector2(0f, -80f);
    [SerializeField] private float scaleFactor = 0.8f;
    

    // -- COMPONENT REFERENCES --
    private CanvasGroup cg;
    private RectTransform rt;

    private Vector2 originalPosition;

    // -- PUBLIC METHODS --

    public void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rt = GetComponent<RectTransform>();
        originalPosition = rt.anchoredPosition;

        if (cg == null && (showAnimation == AnimationType.Fade || showAnimation == AnimationType.FadeAndSlide
                || hideAnimation == AnimationType.Fade || hideAnimation == AnimationType.FadeAndSlide))
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }

        if (showAnimation == AnimationType.Fade || showAnimation == AnimationType.FadeAndSlide)
        {
            cg.alpha = 0f;
        }

        if (showAnimation == AnimationType.Slide || showAnimation == AnimationType.FadeAndSlide)
        {
            rt.anchoredPosition = originalPosition + slideOffset;
        }
    }

    public void SetOriginalPosition(Vector2 position)
    {
        originalPosition = position;
        if (showAnimation == AnimationType.Slide || showAnimation == AnimationType.FadeAndSlide)
        {
            rt.anchoredPosition = originalPosition + slideOffset;
        } else
        {
            rt.anchoredPosition = originalPosition;
        }
    }

    public async Task Show()
    {
        await ShowRoutine();
    }

    public async Task Hide()
    {
        await HideRoutine();
    }

    // -- PRIVATE METHODS --

    private async Task ShowRoutine()
    {
        gameObject.SetActive(true);

        if (showAnimation == AnimationType.Fade || showAnimation == AnimationType.FadeAndSlide)
        {
            cg.alpha = 0f;
        }

        if (showAnimation == AnimationType.Slide || showAnimation == AnimationType.FadeAndSlide)
        {
            rt.anchoredPosition = originalPosition + slideOffset;
        }

        List<Task> animations = new();
        if (showAnimation == AnimationType.Fade || showAnimation == AnimationType.FadeAndSlide)
        {
            animations.Add(UIAnimator.FadeTo(cg, 1f, showDuration, showEasing));
        }
        if (showAnimation == AnimationType.Slide || showAnimation == AnimationType.FadeAndSlide)
        {
            animations.Add(UIAnimator.SlideTo(rt, originalPosition, showDuration, showEasing));
        }

        await Task.WhenAll(animations);
    }

    private async Task HideRoutine()
    {
        List<Task> animations = new();
        if (hideAnimation == AnimationType.Fade || hideAnimation == AnimationType.FadeAndSlide)
        {
            animations.Add(UIAnimator.FadeTo(cg, 0f, hideDuration, hideEasing));
        }
        if (hideAnimation == AnimationType.Slide || hideAnimation == AnimationType.FadeAndSlide)
        {
            animations.Add(UIAnimator.SlideTo(rt, originalPosition + slideOffset, hideDuration, hideEasing));
        }

        await Task.WhenAll(animations);
        gameObject.SetActive(false);
    }
}
