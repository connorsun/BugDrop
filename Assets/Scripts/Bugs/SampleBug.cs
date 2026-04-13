using UnityEngine;


public class SampleBug : Bug
{
    public override BugInfo GetInfo()
    {
        return new BugInfo("SampleBug", 1);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
