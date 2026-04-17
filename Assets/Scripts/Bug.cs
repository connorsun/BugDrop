using UnityEngine;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
        internal static class IsExternalInit {}
}

public abstract class Bug : MonoBehaviour
{

    // All Bug subclasses must implement the GetInfo static method to return their BugInfo!
    public record BugInfo(string name, int rarity, int baseScore, float safeHorizRadius);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // --- CONSTANTS ---
    private const int CONTACT_ARRAY_SIZE = 10;
    // --- PRIVATE STATE ---

    protected bool isActive;
    protected BugInfo thisBugInfo;
    protected bool primaryTriggered;
    protected bool secondaryTriggered;
    // --- OBJECT REFERENCES ---
    [SerializeField] protected Collider2D collider;

    public virtual void Start()
    {
        this.isActive = false;
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Reset overall state to initial spawn
    public virtual void Reset()
    {
        this.primaryTriggered = false;
        this.secondaryTriggered = false;
    }

    // Reset round state on the start of a scoring round
    public virtual void StartScoring()
    {
        this.primaryTriggered = false;
        this.secondaryTriggered = false;
    }

    public virtual async Task Trigger(bool isPrimary)
    {
        if (isPrimary)
        {
            if (this.primaryTriggered)
            {
                return;
            }
            this.primaryTriggered = true;
        } else
        {
            if (this.secondaryTriggered)
            {
                return;
            }
            this.secondaryTriggered = true;
        }
        await this.Score();
        float timestamp = Time.unscaledTime;
        while (Time.unscaledTime < timestamp + 0.5f)
        {
            await Task.Yield();
        }
    }

    // Scores this bug. Runs all operations for scoring this bug's turn before returning.
    protected virtual async Task Score()
    {
        ScorePoints(this.thisBugInfo.baseScore);
    }

    // --- PRIVATE METHODS ---

    // Scores a specific number of points for this round.
    protected void ScorePoints(int score)
    {
        GameHandler.RoundScore += score;
        GameHandler.SingletonUIHandler.UpdateScoreState();
    }

    protected ContactPoint2D[] GetContacts()
    {
        int size = CONTACT_ARRAY_SIZE;
        ContactPoint2D[] contacts = new ContactPoint2D[size];
        int numFilled = size + 1;
        while (numFilled > size) {
            contacts = new ContactPoint2D[size];
            numFilled = collider.GetContacts(contacts);
        }
        return contacts;
    }
}
