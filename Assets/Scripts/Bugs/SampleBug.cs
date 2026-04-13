using UnityEngine;


public class SampleBug : Bug
{

    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("SampleBug", 1, 1f);
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
