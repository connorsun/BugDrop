using UnityEngine;
using System.Threading.Tasks;

public class BugCollision : MonoBehaviour
{
    private const float BUG_HIT_IMPULSE_THRESH = 2f;
    // Threshold for dot product between velocity vector and direction of contact point from center
    private const float BUG_HIT_BUG_THRESH = 3f; //3
    private const float BUG_HIT_GROUND_THRESH = 3f; //3
    
    // Threshold for angle coherence between velocity vector and direction of contact point from center
    private const float BUG_DOT_BUG_THRESH = 0.3f; //0.1
    private const float BUG_DOT_GROUND_THRESH = 0.3f; //0.1
    
    // Buffer times for not playing a sound
    private const float THIS_BUG_SOUND_BUFFER_TIME = 0.04f; //0.04
    private const float OTHER_BUG_SOUND_BUFFER_TIME = 0.4f; //0.5
    
    // Multiplied by angular velocity and added into dot product to override BUG_HIT thresholds
    private const float BUG_ROTATION_WEIGHT = 0.0085f; //0.00875
    
    // Debug ray
    private const float DEBUG_RAY_LENGTH = 1f;

    public Rigidbody2D rb;
    private Vector2 vel;
    private Vector2 prevVel;
    private float angVel;
    private float prevAngVel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    // Update is called once per frame
    void Update()
    {
        prevVel = vel;
        prevAngVel = angVel;
        //Debug.DrawRay(transform.position, prevVel * DEBUG_RAY_LENGTH, Color.green);
        vel = rb.linearVelocity;
        angVel = rb.angularVelocity;
    }

    private async Task DrawRay(Vector2 start, Vector2 finish, Color c)
    {
        for (int i = 0; i < 200; i++) {
            Debug.DrawRay(start, finish, c);
            await Task.Yield();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (rb == null)
        {
            return;
        }
        Bug thisBug = this.GetComponentInParent<Bug>();
        Bug otherBug = other.collider.gameObject.GetComponentInParent<Bug>();
        
        //Buffer time
        float thisCollisionSoundTimestamp = thisBug.GetCollisionSoundTimestamp();
        if (Time.unscaledTime - thisCollisionSoundTimestamp < THIS_BUG_SOUND_BUFFER_TIME)
        {
            return;
        }

        float weightedPrevAngVel = Mathf.Abs(prevAngVel) * BUG_ROTATION_WEIGHT;

        // If contact made
        if (other.contactCount > 0)
        {
            ContactPoint2D contactPoint = other.GetContact(0);
            Vector2 pointDir = (contactPoint.point - (Vector2) transform.position).normalized;

            // DrawRay(contactPoint.point, pointDir * contactPoint.normalImpulse, Color.red);

            if (contactPoint.normalImpulse > BUG_HIT_IMPULSE_THRESH
            || Vector2.Dot(prevVel, pointDir) /*+ weightedPrevAngVel*/ > BUG_HIT_BUG_THRESH)
            //    && Vector2.Dot(prevVel.normalized, pointDir) > BUG_DOT_BUG_THRESH)
            {
                // If bug hit or ground hit

                if (otherBug != null && otherBug != thisBug) // Bug hit
                {
                    float otherCollisionSoundTimestamp = otherBug.GetCollisionSoundTimestamp();
                    if (Time.unscaledTime - otherCollisionSoundTimestamp < OTHER_BUG_SOUND_BUFFER_TIME)
                    {
                        return;
                    }

                    // DrawRay(contactPoint.point, pointDir * contactPoint.normalImpulse, Color.green);
                    GameHandler.PlaySound("Bug Hit Other Bug");

                    thisBug.SetCollisionSoundTimestamp(Time.unscaledTime);
                    otherBug.SetCollisionSoundTimestamp(Time.unscaledTime);
                    print("pronk " + thisBug.thisBugInfo.name);
                } else if (otherBug == null) // Ground hit
                {
                    // DrawRay(contactPoint.point, pointDir * contactPoint.normalImpulse, Color.green);
                    GameHandler.PlaySound("Bug Hit Ground");

                    thisBug.SetCollisionSoundTimestamp(Time.unscaledTime);
                }
            }
        }
    }
}
