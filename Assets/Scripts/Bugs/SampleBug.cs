using UnityEngine;
using System.Threading.Tasks;


public class SampleBug : Bug
{
    // --- CONSTANTS ---
    private const int CONTACT_ARRAY_SIZE = 10;
    // --- OBJECT REFERENCES --- 
    [SerializeField] private Collider2D collider;

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("SampleBug", 1, 1f);
    }

    // --- PUBLIC METHODS ---

    public override async Task Score()
    {
        GameHandler.RoundScore++;
        // float timestamp = Time.unscaledTime;
        // while (Time.unscaledTime < timestamp + 0.5f)
        // {
        //     await Task.Yield();
        // }
    }
}
