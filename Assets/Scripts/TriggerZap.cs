using UnityEngine;

public class TriggerZap : MonoBehaviour
{
    public Vector2 start;
    public Vector2 end;
    public float timeToLive;
    private float startStamp;
    [SerializeField] private LineRenderer line;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init()
    {
        startStamp = Time.unscaledTime;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.unscaledTime > startStamp + timeToLive)
        {
            Destroy(gameObject);
        }
    }
}
