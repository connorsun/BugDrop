using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
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

    [SerializeField] private bool hasOriginalPosition = false;
    [SerializeField] private Vector2 originalPosition = Vector2.negativeInfinity;

    private Button button;

    // -- PUBLIC METHODS --

    public void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rt = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        
        if (!hasOriginalPosition)
        {
            originalPosition = rt.anchoredPosition;
        }

        if (cg == null && (showAnimation == AnimationType.Fade || showAnimation == AnimationType.FadeAndSlide
                || hideAnimation == AnimationType.Fade || hideAnimation == AnimationType.FadeAndSlide))
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }

        if (button != null){
            cg.interactable = false;
            cg.blocksRaycasts = false;
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
        if (button != null){
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        await ShowRoutine();
    }

    public async Task Hide()
    {
        if (button != null){
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
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
