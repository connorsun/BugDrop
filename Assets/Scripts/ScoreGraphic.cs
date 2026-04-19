using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class ScoreGraphic : MonoBehaviour
{
    // public float timeToLive;
    // private float startStamp;
    // private float initialY;
    // private const float Y_SPEED = 15f;

    private UIAnimatable anim;
    [SerializeField] private TextMeshProUGUI[] textMeshPros;

    public void Init()
    {
        // startStamp = Time.unscaledTime;
        // initialY = transform.position.y;
    }

    public async void Start()
    {
        anim = GetComponent<UIAnimatable>();
        await Animate();
        Destroy(gameObject);
    }

    public void SetText(int score)
    {
        foreach (TextMeshProUGUI child in textMeshPros)
        {
            child.text = (score >= 0 ? "+" : "") + score;
        }
    }

    public void SetColor(bool isPrimary)
    {
        textMeshPros[textMeshPros.Length - 1].color = isPrimary ? GameHandler.PRIMARY_COLOR : GameHandler.SECONDARY_COLOR;
    }

    // Update is called once per frame
    void Update()
    {
        // transform.position = new Vector2(transform.position.x, initialY + (Time.unscaledTime - startStamp) * Y_SPEED);
        // if (Time.unscaledTime > startStamp + timeToLive)
        // {
        //     Destroy(gameObject);
        // }
    }

    public async Task Animate()
    {
        await anim.Show();
        await anim.Hide();
    }
}