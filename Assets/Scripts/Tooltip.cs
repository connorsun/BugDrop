using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tooltipText;
    private const float DETECTION_RADIUS = 0.05f;
    private const float X_MARGIN = 16;
    private const float Y_MARGIN = 16;
    private Vector4 baseMargins;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseMargins = tooltipText.margin;
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
                tooltipText.text = "[" + bug.baseScore + "] " + bug.thisBugInfo.tooltip;
                transform.position = mouseWorldPos;
                UpdateBounds();
                return;
            }
        }
        tooltipText.text = "";
        UpdateBounds();
    }
    private void UpdateBounds()
    {
        tooltipText.ForceMeshUpdate();
        Bounds textBounds = tooltipText.textBounds;
        print(textBounds);
        if (textBounds.min.y < Y_MARGIN)
        {
            tooltipText.margin = baseMargins + new Vector4(0f, textBounds.min.y - Y_MARGIN, 0f, 0f);
        } else
        {
            tooltipText.margin = baseMargins;
        }
        
    }
}
