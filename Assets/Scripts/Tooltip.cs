using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private float flipMarginX = 160f;
    [SerializeField] private float flipMarginY = 60f;
    private const float X_MARGIN = 16;
    private const float Y_MARGIN = 16;
    private Vector4 baseMargins;
    private CanvasGroup cg;
    private RectTransform rt;

    private Bug currentBug;
    private Bug prevBug;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseMargins = tooltipText.margin;
        cg = GetComponent<CanvasGroup>();
        rt = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameHandler.MovingBug == null) {
            List<Collider2D> overlapColliders = new List<Collider2D>();
            Vector3 mouseWorldPos = GameHandler.GetMouseWorldPos();
            Physics2D.OverlapCircle(mouseWorldPos, GameHandler.MOUSE_DETECTION_RADIUS, ContactFilter2D.noFilter, overlapColliders);
            foreach (Collider2D col in overlapColliders)
            {
                Bug bug = col.gameObject?.GetComponentInParent<Bug>();
                if (bug != null && bug.IsStationary())
                {
                    if (prevBug != bug)
                    {
                        if (prevBug != null) _ = prevBug.Hover(false, 0f, true);
                        prevBug = bug;
                        _ = bug.Hover(true, 0.3f, true);
                    }

                    cg.alpha = 1;
                    titleText.text = bug.thisBugInfo.name;
                    tooltipText.text = "[Base " + bug.baseScore + (bug.baseScore == 1 ? " point] " : " points] ") + bug.thisBugInfo.tooltip;
                    tooltipText.ForceMeshUpdate();
                    int lineCount = tooltipText.textInfo.lineCount;
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, 25 + (lineCount * 8));

                    float scale = 0.0625f;
                    float w = (((rt.sizeDelta.x + bug.thisBugInfo.safeHorizRadius) / 2) + 8) * scale;
                    float h = (((rt.sizeDelta.y + bug.thisBugInfo.safeVertRadius) / 2) + 8) * scale;

                    float xOffset = ((bug.center.position.x + w) > (flipMarginX * scale)) ? -w : w;
                    float yOffset = ((bug.center.position.y + h) > (flipMarginY * scale)) ? -h : h;

                    transform.position = new Vector3(
                        bug.center.position.x + xOffset,   // shift right by tooltip width
                        bug.center.position.y + yOffset,   // shift up by tooltip height
                        bug.center.position.z
                    );

                    return;
                }
            }

            if (prevBug != null)
            {
                _ = prevBug.Hover(false, 0f, true);
                prevBug = null;
            }

            cg.alpha = 0;
        } else
        {
            cg.alpha = 0;
            prevBug = null;
        }
        //UpdateBounds();
    }
    // private void UpdateBounds()
    // {
    //     tooltipText.ForceMeshUpdate();
    //     Bounds textBounds = tooltipText.textBounds;
    //     print(textBounds);
    //     if (textBounds.min.y < Y_MARGIN)
    //     {
    //         tooltipText.margin = baseMargins + new Vector4(0f, textBounds.min.y - Y_MARGIN, 0f, 0f);
    //     } else
    //     {
    //         tooltipText.margin = baseMargins;
    //     }
        
    // }
}
