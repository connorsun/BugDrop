using UnityEngine;
public class KnockoutProgressBar : MonoBehaviour
{
    private enum Type {
        Snap,
        Lerp,
        Label
    }

    [SerializeField] private Type type;
    private Vector3 startingPos;
    private float loadingBarLength;
    private UIAnimatable ui;
    public bool allowedToMove;
    Vector3 targetPos;

    public void Start() {
        startingPos = transform.localPosition;
        if (type == Type.Label)
        {
            loadingBarLength = 256;
        } else
        {
            loadingBarLength = ((RectTransform) transform.parent).sizeDelta.x;
        }
    }

    public void Update() {
        if (allowedToMove)
        {
            if (type == Type.Label)
            {
                targetPos = new Vector3(-128, 152, 0) + Vector3.right * (loadingBarLength * Mathf.Min(GameHandler.RoundScore / GameHandler.ScoreThreshold, 1));
            } else
            {
                targetPos = startingPos + Vector3.right * (loadingBarLength * GameHandler.RoundScore / GameHandler.ScoreThreshold);
            }
        }

        if (type == Type.Lerp || type == Type.Label)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, 0.06f);
        } else
        {
            transform.localPosition = targetPos;
        }
    }
}