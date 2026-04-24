using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;


public class Spider : Bug
{
    // --- CONSTANTS ---
    // --- OBJECT REFERENCES --- 
    public Transform thoraxPoint;
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
    private GameObject spiderLine;

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
                    if (this.spiderLine == null) {
                        this.spiderLine = Instantiate(GameHandler.GetResource("Prefabs/SpiderLine") as GameObject);
                    }
                    this.spiderLine.GetComponent<SpiderLine>().spider1 = this;
                    this.spiderLine.GetComponent<SpiderLine>().spider2 = this.pairedSpider;
                    this.spiderLine.GetComponent<SpiderLine>().SetColor(new Color(1f, 1f, 1f, 0.5f));
                    break;
                }
            }
        }
        base.Start();
    }

    public override void Destroy()
    {
        if (spiderLine != null) {
            DestroyImmediate(spiderLine);
        }
        base.Destroy();
    }

    public override async Task Hover(bool on, float intensity, bool affectOthers)
    {
        base.Hover(on, intensity, affectOthers);
        this.spiderLine.GetComponent<SpiderLine>().SetColor(new Color(1f, 1f, 1f, on? 1f : 0.5f));
    }

    public override Bug[] GetAffectedBugs()
    {
        if (this.pairedSpider != null) {
            List<RaycastHit2D> rayHits = new List<RaycastHit2D>();
            Vector2 toOther = (Vector2) (this.pairedSpider.thoraxPoint.position - this.thoraxPoint.position);
            Physics2D.Raycast(this.thoraxPoint.position, toOther.normalized, ContactFilter2D.noFilter, rayHits, toOther.magnitude);
            List<Bug> bugsScored = new List<Bug>();
            foreach (RaycastHit2D rayHit in rayHits)
            {
                Bug otherBug = rayHit.collider?.gameObject?.GetComponentInParent<Bug>();
                print(otherBug);
                //print(otherBug);
                if (otherBug != null && otherBug != this && otherBug != this.pairedSpider)
                {
                    bugsScored.Add(otherBug);
                    
                }
            }
            return bugsScored.ToArray();
        }
        return new Bug[0];
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
                foreach (Bug bug in GetAffectedBugs())
                {
                    totalScore += bug.CalculateOverallScore(this);
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
