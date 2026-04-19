using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace System.Runtime.CompilerServices
{
        internal static class IsExternalInit {}
}

public abstract class Bug : MonoBehaviour
{

    // All Bug subclasses must implement the GetInfo static method to return their BugInfo!
    public record BugInfo(string name, int rarity, int baseScore, float safeHorizRadius, float safeVertRadius, string tooltip);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // --- CONSTANTS ---
    private const int CONTACT_ARRAY_SIZE = 10;
    private const float STATIONARY_MAG_THRESHOLD = 0.05f;
    private const int STATIONARY_FRAMES_THRESHOLD = 15;

    // Recursive secondary triggering - if we want retriggers to be really strong
    public const bool RECURSIVE_SECONDARIES = true;
    public BugInfo thisBugInfo;
    // --- PUBLIC STATE ---
    public bool primaryTriggered;
    public bool secondaryTriggered;
    public int baseScore;
    // --- PRIVATE STATE ---

    protected bool isActive;
    private int stationaryFrames;
    // --- OBJECT REFERENCES ---

    // Center must be used as the main transform to get the position of the bug, as the
    // parent object this script is on does not follow the position of the child segments
    [SerializeField] protected Transform center;
    [SerializeField] protected Collider2D[] colliders;
    [SerializeField] protected Rigidbody2D[] rigidbodies;

    public virtual void Start()
    {
        this.isActive = false;
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Reset round state on the start of a placing round
    public virtual void StartPlacing()
    {
        this.stationaryFrames = 0;
        this.baseScore = this.thisBugInfo.baseScore;
    }
    // Reset round state on the start of a scoring round
    public virtual void StartScoring()
    {
        this.primaryTriggered = false;
        this.secondaryTriggered = false;
        this.stationaryFrames = 0;
    }

    // Reset overall state at the end of scoring round
    public virtual void Reset()
    {
        this.primaryTriggered = false;
        this.secondaryTriggered = false;
        this.stationaryFrames = 0;
        this.baseScore = this.thisBugInfo.baseScore;
    }

    public virtual async Task Trigger(bool isPrimary, Vector3 prevPos)
    {
        if (isPrimary)
        {
            if (this.primaryTriggered)
            {
                return;
            }
            this.primaryTriggered = true;
            if (RECURSIVE_SECONDARIES)
            {
                GameHandler.BroadcastToBugs((Bug bug) => bug.secondaryTriggered = false);
                this.secondaryTriggered = true;
            }
        } else
        {
            if (this.secondaryTriggered)
            {
                return;
            }
            this.secondaryTriggered = true;
            await Task.Delay(TimeSpan.FromSeconds(0.4f * GameHandler.GameSpeed));
        }
        GameObject zap = Instantiate(GameHandler.GetResource("Prefabs/TriggerZap") as GameObject);
        Color zapColor = isPrimary? GameHandler.PRIMARY_COLOR : GameHandler.SECONDARY_COLOR;
        zap.GetComponent<LineRenderer>().startColor = zapColor;
        zap.GetComponent<LineRenderer>().endColor = zapColor;
        TriggerZap tZap = zap.GetComponent<TriggerZap>();
        tZap.start = (Vector2) prevPos;
        tZap.end = (Vector2) center.position;
        tZap.timeToLive = isPrimary? 0.6f * GameHandler.GameSpeed : 0.4f * GameHandler.GameSpeed;
        tZap.Init();
        await this.Score(isPrimary);
        if (isPrimary) {
            Bug[] closest = GetClosestBugs();
            if (closest.Length > 0) {
                await Task.Delay(TimeSpan.FromSeconds(0.6f * GameHandler.GameSpeed));
                foreach (Bug bug in closest)
                {
                    if (!bug.primaryTriggered)
                    {
                        if (!bug.primaryTriggered)
                        {
                            await bug.Trigger(true, center.position);
                            return;
                        }
                    }
                }
            }
        }
    }

    // Checks if this bug is stationary
    public virtual bool IsStationary()
    {
        Rigidbody2D centerRb = center.gameObject.GetComponent<Rigidbody2D>();
        if (centerRb.linearVelocity.magnitude < STATIONARY_MAG_THRESHOLD)
        {
            stationaryFrames++;
            if (stationaryFrames >= STATIONARY_FRAMES_THRESHOLD)
            {
                return true;
            }
        } else
        {
            stationaryFrames = 0;
        }
        return false;
    }

    // Turns on and off simulation for this bug's rigidbodies
    public virtual void SetSimulated(bool isSimulated)
    {
        foreach (Rigidbody2D rb in rigidbodies)
        {
            rb.simulated = isSimulated;
        }
    }

    public virtual void Destroy()
    {
        DestroyImmediate(gameObject);
    }

    // --- PRIVATE METHODS ---

    // Scores this bug. Runs all operations for scoring this bug's turn before returning.
    protected abstract Task Score(bool isPrimary);


    // Scores a specific number of points for this round.
    protected void ScorePoints(int score, bool isPrimary)
    {
        GameHandler.RoundScore += score;
        GameHandler.SingletonUIHandler.UpdateScoreState();
        GameHandler.SingletonUIHandler.CreateScoreGraphic(
            center.transform.position + 
                    new Vector3(0.5f, 0.5f) +
                    //new Vector3(this.thisBugInfo.safeHorizRadius / 2f, this.thisBugInfo.safeVertRadius) +
                    new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f), 0f),
            score, isPrimary);
        GameHandler.PlaySound("TextboxSkip");
    }

    protected ContactPoint2D[] GetContacts()
    {
        // Dictionary ensures there is only 1 contact point stored per other bug
        Dictionary<Bug, ContactPoint2D> bugContacts = new Dictionary<Bug, ContactPoint2D>();
        List<ContactPoint2D> otherContacts = new List<ContactPoint2D>();

        // Iterate over all child colliders
        foreach (Collider2D col in colliders)
        {
            int size = CONTACT_ARRAY_SIZE;
            ContactPoint2D[] contacts = new ContactPoint2D[size];
            int numFilled = size + 1;
            while (numFilled > size) {
                if (size != CONTACT_ARRAY_SIZE)
                {
                    size *= 2;
                }
                contacts = new ContactPoint2D[size];
                numFilled = col.GetContacts(contacts);
            }

            // Discern which type of contact point this is
            for (int i = 0; i < numFilled; i++)
            {
                ContactPoint2D contact = contacts[i];
                Bug other = contact.collider.GetComponentInParent<Bug>();
                
                if (other == this) continue;

                if (other == null)
                {
                    otherContacts.Add(contact);
                    continue;
                }

                if (!bugContacts.ContainsKey(other))
                {
                    bugContacts[other] = contact;
                }
            }
        }

        // Merge unique bug contact points with all other contacts (walls, floor, etc.)
        ContactPoint2D[] result = new ContactPoint2D[bugContacts.Count + otherContacts.Count];
        bugContacts.Values.CopyTo(result, 0);
        otherContacts.CopyTo(result, bugContacts.Count);
        
        return result;
    }

    private Bug[] GetClosestBugs()
    {
        return GameHandler.AllBugs.OrderBy(x => (x.center.position - center.position).magnitude).ToArray();
    }
}
