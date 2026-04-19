using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tooltipText;
    private const float DETECTION_RADIUS = 0.05f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        List<Collider2D> overlapColliders = new List<Collider2D>();
        Vector3 mouseWorldPos = GameHandler.GetMouseWorldPos();
        Physics2D.OverlapCircle(mouseWorldPos, DETECTION_RADIUS, ContactFilter2D.noFilter, overlapColliders);
        foreach (Collider2D col in overlapColliders)
        {
            Bug bug = col.gameObject?.GetComponentInParent<Bug>();
            if (bug != null)
            {
                tooltipText.text = "[" + bug.thisBugInfo.baseScore + "] " + bug.thisBugInfo.tooltip;
                transform.position = mouseWorldPos;
                return;
            }
        }
        tooltipText.text = "";
    }
}
