using UnityEngine;
public class KnockoutProgressBar : MonoBehaviour
{
    private Vector3 startingPos;
    private float loadingBarLength;
    public  void Start() {
        startingPos = transform.localPosition;
        loadingBarLength = ((RectTransform) transform.parent).sizeDelta.x;
    }
    public void Update() {
        transform.localPosition = startingPos + Vector3.right * (loadingBarLength * GameHandler.RoundScore / GameHandler.ScoreThreshold);
    }
}