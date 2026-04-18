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
    public record BugInfo(string name, int rarity, int baseScore, float safeHorizRadius);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // --- CONSTANTS ---
    private const int CONTACT_ARRAY_SIZE = 10;

    // Recursive secondary triggering - if we want retriggers to be really strong
    public const bool RECURSIVE_SECONDARIES = true;
    // --- PRIVATE STATE ---

    protected bool isActive;
    protected BugInfo thisBugInfo;
    public bool primaryTriggered;
    public bool secondaryTriggered;
    // --- OBJECT REFERENCES ---
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
        }
        GameObject zap = Instantiate(GameHandler.GetResource("Prefabs/TriggerZap") as GameObject);
        Color zapColor = isPrimary? Color.yellow : Color.blue;
        zap.GetComponent<LineRenderer>().startColor = zapColor;
        zap.GetComponent<LineRenderer>().endColor = zapColor;
        TriggerZap tZap = zap.GetComponent<TriggerZap>();
        tZap.start = (Vector2) prevPos;
        tZap.end = (Vector2) transform.position;
        tZap.timeToLive = 0.3f;
        tZap.Init();
        await this.Score();
        if (isPrimary) {
            float timestamp = Time.unscaledTime;
            while (Time.unscaledTime < timestamp + 0.5f)
            {
                await Task.Yield();
            }
            foreach (Bug bug in GetClosestBugs())
            {
                if (!bug.primaryTriggered)
                {
                    await bug.Trigger(true, transform.position);
                    return;
                }
            }
        }
    }

    // Scores this bug. Runs all operations for scoring this bug's turn before returning.
    protected virtual async Task Score()
    {
        ScorePoints(this.thisBugInfo.baseScore);
    }

    // Turns on and off simulation for this bug's rigidbodies
    public virtual void SetSimulated(bool isSimulated)
    {
        foreach (Rigidbody2D rb in rigidbodies)
        {
            rb.simulated = isSimulated;
        }
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
        // Dictionary ensures there is only 1 contact point stored per other bug
        Dictionary<Bug, ContactPoint2D> bugContacts = new Dictionary<Bug, ContactPoint2D>();
        List<ContactPoint2D> otherContacts = new List<ContactPoint2D>();

        // Iterate over all child colliders
        foreach (Collider2D col in colliders)
        {
            int size = CONTACT_ARRAY_SIZE;
            ContactPoint2D[] contacts = new ContactPoint2D[size];
            int numFilled = size + 1;
            print("collider " + col.gameObject.name);
            while (numFilled > size) {
                print(size);
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
                Bug other = contact.otherCollider.GetComponentInParent<Bug>();
                
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
        print(string.Join(",", result));
        
        return result;
    }

    private Bug[] GetClosestBugs()
    {
        return GameHandler.AllBugs.OrderBy(x => (x.transform.position - transform.position).magnitude).ToArray();
    }
}
