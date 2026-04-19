using UnityEngine;

public class TriggerZap : MonoBehaviour
{
    public Vector2 start;
    public Vector2 end;
    public float timeToLive;
    private float startStamp;
    [SerializeField] private LineRenderer line;

    [Header("Animation")]
    [SerializeField] private Texture2D[] frames;
    [SerializeField] private int sprites;
    [SerializeField] private float animFps = 12f;
    private float animTimer;
    private int currentFrame;
    private Material lineMaterial;

    [Header("Width Lerping")]
    [SerializeField] private float fadeInDuration = 0.1f;
    [SerializeField] private float colorFadeDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float lerpFps = 18f;
    private float lerpTimer;

    private float startWidth;
    private float endWidth;
    private Color color;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init()
    {
        startStamp = Time.unscaledTime;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        lineMaterial = line.material;
        lineMaterial.mainTexture = frames[0];

        startWidth = line.startWidth;
        endWidth = line.endWidth;
        line.startWidth = 0f;
        line.endWidth = 0f;

        color = line.startColor;
    }

    // Update is called once per frame
    void Update()
    {
        float elapsed = Time.unscaledTime - startStamp;
        float t = elapsed / timeToLive;

        if (t > 1f)
        {
            Destroy(gameObject);
            return;
        }

        UpdateWidth(t);
        UpdateAnimation();
    }

    void UpdateColor(float t)
    {
        line.startColor = t < colorFadeDuration ? Color.white : color;
        line.endColor = t < colorFadeDuration ? Color.white : color;
    }

    void UpdateWidth(float t)
    {
        lerpTimer += Time.deltaTime;
        if (lerpTimer < 1f / lerpFps) return;

        lerpTimer = 0;

        float w;

        if (t < fadeInDuration)
            w = t / fadeInDuration;
        else if (t > 1f - fadeOutDuration)
            w = 1f - (t - (1f - fadeOutDuration)) / fadeOutDuration;
        else
            w = 1f;

        line.startWidth = startWidth * w;
        line.endWidth = endWidth * w;

        UpdateColor(t);
    }

    void UpdateAnimation()
    {
        animTimer += Time.deltaTime;
        if (animTimer < 1f / animFps) return;

        animTimer = 0f;
        currentFrame = (currentFrame + 1) % frames.Length;
        lineMaterial.mainTexture = frames[currentFrame];
    }
}
