using UnityEngine;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
        internal static class IsExternalInit {}
}

public abstract class Bug : MonoBehaviour
{

    // All Bug subclasses must implement the GetInfo static method to return their BugInfo!
    public record BugInfo(string name, int rarity, float safeHorizRadius);
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    protected bool isActive;

    void Start()
    {
        isActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void Reset()
    {
        
    }


    public virtual void StartScoring()
    {
        
    }
    public virtual async Task Score()
    {
        
    }
}
