using UnityEngine;

public class SpiderLine : MonoBehaviour
{
    public Spider spider1;
    public Spider spider2;
    [SerializeField] private LineRenderer line;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetPosition();
    }

    // Update is called once per frame
    void Update()
    {
        SetPosition();
    }
    private void SetPosition()
    {
        if (spider1 != null)
        {
            line.SetPosition(0, spider1.thoraxPoint.position);
            line.SetPosition(1, spider2.thoraxPoint.position);
        }   
    }
    public void SetColor(Color color)
    {
        line.startColor = color;
        line.endColor = color;
    }
}
