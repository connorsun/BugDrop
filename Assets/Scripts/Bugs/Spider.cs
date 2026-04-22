using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;


public class Spider : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 

    // --- STATIC METADATA ---
    // Gets metadata about this bug type
    public static BugInfo GetInfo()
    {
        return new BugInfo("Spider", 3, 0, 1.5f, 1.5f, "Sums half the score of all bugs between this and a paired Spider");
    }
    // --- PRIVATE STATE ---
    public int spiderNum;
    public Spider pairedSpider;
    private bool isCached;
    private float cachedScore;

    // --- PUBLIC METHODS ---
    public override void Start()
    {
        this.thisBugInfo = GetInfo();
        Spider[] spiders = FindObjectsByType<Spider>();
        spiderNum = spiders.Length;
        if (Mathf.Abs(spiderNum) % 2 == 0)
        {
            foreach (Spider spider in spiders)
            {
                print(spider.spiderNum);
                if (spider.spiderNum == spiderNum - 1)
                {
                    this.pairedSpider = spider;
                    spider.pairedSpider = this;
                    break;
                }
            }
        }
        base.Start();
    }

    public override void Reset()
    {
        base.Reset();
        this.isCached = false;
    }

    public override void StartPlacing()
    {
        base.StartPlacing();
        this.isCached = false;
    }

    public override float CalculateOverallScore()
    {
        float totalScore = this.baseScore;
        if (isCached)
        {
            totalScore = this.cachedScore;
        } else {
            if (this.pairedSpider != null) {
                List<RaycastHit2D> rayHits = new List<RaycastHit2D>();
                Vector2 toOther = (Vector2) (this.pairedSpider.center.position - this.center.position);
                Physics2D.Raycast(this.center.position, toOther.normalized, ContactFilter2D.noFilter, rayHits, toOther.magnitude);
                List<Bug> bugsScored = new List<Bug>();
                foreach (RaycastHit2D rayHit in rayHits)
                {
                    Bug otherBug = rayHit.collider?.gameObject?.GetComponentInParent<Bug>();
                    print(otherBug);
                    //print(otherBug);
                    if (otherBug != null && otherBug != this && otherBug != this.pairedSpider)
                    {
                        // will need to add some anti recursive stuff
                        bugsScored.Add(otherBug);
                        totalScore += otherBug.CalculateOverallScore(this);
                    }
                    if (otherBug == this.pairedSpider)
                    {
                        break;
                    }
                }
                this.cachedScore = totalScore;
                isCached = true;
            }
        }
        return (totalScore / 2f) * this.multiplier;
    }

    protected override async Task Score(bool isPrimary, int recursiveSecondaries)
    {
        ScorePoints(CalculateOverallScore(), isPrimary);
    }
}
