using UnityEngine;
using System.Threading.Tasks;

public class ScoreGraphic : MonoBehaviour
{
    // public float timeToLive;
    // private float startStamp;
    // private float initialY;
    // private const float Y_SPEED = 15f;

    private UIAnimatable anim;

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