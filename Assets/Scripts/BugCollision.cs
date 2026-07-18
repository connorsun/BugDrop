using UnityEngine;

public class BugCollision : MonoBehaviour
{
    private const float BUG_HIT_BUG_THRESH = 3f;
    private const float BUG_DOT_BUG_THRESH = 0.1f;
    private const float BUG_HIT_GROUND_THRESH = 3f;
    private const float BUG_DOT_GROUND_THRESH = 0.1f;
    private const float THIS_BUG_SOUND_BUFFER_TIME = 0.04f;
    private const float OTHER_BUG_SOUND_BUFFER_TIME = 0.5f;
    private const float BUG_ROTATION_WEIGHT = 0.00875f;
    public Rigidbody2D rb;
    private Vector2 vel;
    private Vector2 prevVel;
    private float angVel;
    private float prevAngVel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        prevVel = vel;
        prevAngVel = angVel;
        vel = rb.linearVelocity;
        angVel = rb.angularVelocity;
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (rb == null)
        {
            return;
        }
        Bug thisBug = this.GetComponentInParent<Bug>();
        Bug otherBug = other.collider.gameObject.GetComponentInParent<Bug>();
        float thisCollisionSoundTimestamp = thisBug.GetCollisionSoundTimestamp();

        if (Time.unscaledTime - thisCollisionSoundTimestamp < THIS_BUG_SOUND_BUFFER_TIME)
        {
            return;
        }

        float sqrtPrevAngVel = Mathf.Abs(prevAngVel) * BUG_ROTATION_WEIGHT;

        if (otherBug != null && otherBug != thisBug)
        {
            float otherCollisionSoundTimestamp = otherBug.GetCollisionSoundTimestamp();

            if (Time.unscaledTime - otherCollisionSoundTimestamp < OTHER_BUG_SOUND_BUFFER_TIME)
            {
                return;
            }

            if (other.contactCount > 0)
            {
                ContactPoint2D contactPoint = other.GetContact(0);
                Vector2 pointDir = (contactPoint.point - (Vector2) transform.position).normalized;
                if (Vector2.Dot(prevVel, pointDir) + sqrtPrevAngVel > BUG_HIT_BUG_THRESH 
                    && Vector2.Dot(prevVel.normalized, pointDir) > BUG_DOT_BUG_THRESH)
                {
                    GameHandler.PlaySound("Bug Hit Other Bug");
                    thisBug.SetCollisionSoundTimestamp(Time.unscaledTime);
                    otherBug.SetCollisionSoundTimestamp(Time.unscaledTime);
                    print("pronk " + thisBug.thisBugInfo.name);
                }
            }
        } else if (otherBug == null) {
            if (other.contactCount > 0)
            {
                ContactPoint2D contactPoint = other.GetContact(0);
                Vector2 pointDir = (contactPoint.point - (Vector2) transform.position).normalized;
                if (Vector2.Dot(prevVel, pointDir) + sqrtPrevAngVel > BUG_HIT_GROUND_THRESH 
                    && Vector2.Dot(prevVel.normalized, pointDir) > BUG_DOT_GROUND_THRESH)
                {
                    print(Vector2.Dot(prevVel, pointDir));
                    GameHandler.PlaySound("Bug Hit Ground");
                    thisBug.SetCollisionSoundTimestamp(Time.unscaledTime);
                }
            }
        }
    }
}
