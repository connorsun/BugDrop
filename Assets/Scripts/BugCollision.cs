using UnityEngine;

public class BugCollision : MonoBehaviour
{
    private const float BUG_HIT_BUG_THRESH = 0.15f;
    private const float BUG_HIT_GROUND_THRESH = 0.15f;
    public Rigidbody2D rb;
    private Vector2 vel;
    private Vector2 prevVel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        prevVel = vel;
        vel = rb.linearVelocity;

    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (rb == null)
        {
            return;
        }
        if (other.collider.gameObject.GetComponentInParent<Bug>() != null)
        {
            if (other.contactCount > 0)
            {
                ContactPoint2D contactPoint = other.GetContact(0);
                Vector2 pointDir = (contactPoint.point - (Vector2) transform.position).normalized;
                print("bug " + Vector2.Dot(prevVel, pointDir));
                if (Vector2.Dot(prevVel, pointDir) > BUG_HIT_BUG_THRESH)
                {
                    GameHandler.PlaySound("Bug Hit Other Bug");
                }
            }
        } else {
            if (other.contactCount > 0)
            {
                ContactPoint2D contactPoint = other.GetContact(0);
                Vector2 pointDir = (contactPoint.point - (Vector2) transform.position).normalized;
                print("ground " + Vector2.Dot(prevVel, pointDir));
                if (Vector2.Dot(prevVel, pointDir) > BUG_HIT_GROUND_THRESH)
                {
                    GameHandler.PlaySound("Bug Hit Ground");
                }
            }
        }
    }
}
