using UnityEngine;

public class ScoreGraphic : MonoBehaviour
{
    public float timeToLive;
    private float startStamp;
    private float initialY;
    private const float Y_SPEED = 15f;
    public void Init()
    {
        startStamp = Time.unscaledTime;
        initialY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector2(transform.position.x, initialY + (Time.unscaledTime - startStamp) * Y_SPEED);
        if (Time.unscaledTime > startStamp + timeToLive)
        {
            Destroy(gameObject);
        }
    }
}
