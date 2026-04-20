using UnityEngine;

public class Particle : MonoBehaviour
{
    private ParticleSystem ps;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Play();
    }

    private void Update()
    {
        if (!ps.IsAlive())
            Destroy(gameObject);
    }
}