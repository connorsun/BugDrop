using UnityEngine;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
        internal static class IsExternalInit {}
}

public abstract class Bug : MonoBehaviour
{
    public record BugInfo(string name, int rarity);

    // Gets metadata about this bug type
    public abstract BugInfo GetInfo();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
