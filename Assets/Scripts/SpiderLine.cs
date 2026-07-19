using UnityEngine;
using System.Collections.Generic;

public class SpiderLine : MonoBehaviour
{
    public Spider spider1;
    public Spider spider2;
    [SerializeField] private LineRenderer line;
    [SerializeField] private Transform spiderHitbox;
    [SerializeField] private BoxCollider2D lineCol;
    private const float LINE_HITBOX_WIDTH = 0.285f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetPosition();
    }

    // Update is called once per frame
    void Update()
    {
        SetPosition();
    }
    private void SetPosition()
    {
        if (spider1 != null)
        {
            Vector3 pos1 = new Vector3(spider1.thoraxPoint.position.x, spider1.thoraxPoint.position.y, 0f);
            Vector3 pos2 = new Vector3(spider2.thoraxPoint.position.x, spider2.thoraxPoint.position.y, 0f);
            Vector3 dir = pos2 - pos1;
            line.SetPosition(0, pos1);
            line.SetPosition(1, pos2);
            spiderHitbox.position = pos1;
            spiderHitbox.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            lineCol.offset = new Vector2(dir.magnitude/2f, 0f);
            lineCol.size = new Vector2(dir.magnitude, LINE_HITBOX_WIDTH);
        }
    }
    public void SetColor(Color color)
    {
        line.startColor = color;
        line.endColor = color;
    }
    public HashSet<Bug> GetSpiderLineBugs(List<Bug> ignore)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        List<Collider2D> cols = new List<Collider2D>();
        lineCol.Overlap(filter, cols);
        HashSet<Bug> foundBugs = new HashSet<Bug>();
        foreach (Collider2D col in cols)
        {
            Bug bug = col.gameObject?.GetComponentInParent<Bug>();
            if (bug != null && !ignore.Contains(bug))
            {
                foundBugs.Add(bug);
            }
        }
        return foundBugs;
    }
}
