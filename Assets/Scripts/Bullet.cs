using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 80f;
    public float lifeTime = 3f;

    private Rigidbody rb;

    [Header("Impact Sounds")]
    public AudioClip canHitClip;
    public AudioClip boxHitClip;

    [Header("Can Particles ONLY")]
    public ParticleSystem canHitParticle;

    private AudioSource impactAudio;

    Transform target;
    CarShooter shooter;

    // 🔥 NEW (direction lock like Zombie Derby)
    Vector3 shootDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        CreateAudio();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            if (target != null)
            {
                // 🎯 SHOOT TOWARD TARGET (LEFT/RIGHT WORKS HERE)
                shootDirection = (target.position - transform.position).normalized;
            }
            else
            {
                // 🔥 ALWAYS WORLD LEFT (-X)
                shootDirection = Vector3.left;
            }

            rb.velocity = shootDirection * speed;
        }

        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // 🔥 ONLY assist if target exists
        if (target != null)
        {
            Vector3 dir = (target.position - transform.position).normalized;

            // small correction only
            shootDirection = Vector3.Lerp(shootDirection, dir, 0.05f);
            rb.velocity = shootDirection * speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObj = collision.gameObject;

        Vector3 hitPoint = collision.contacts[0].point;

        // 🔥 USE CURRENT VELOCITY DIRECTION (IMPORTANT)
        Vector3 dir = rb.velocity.normalized;

        // 💥 BREAKABLE OBJECT
        BreakableBoard breakable = hitObj.GetComponentInParent<BreakableBoard>();
        if (breakable != null)
        {
            breakable.RegisterBulletHit(hitPoint, dir);
        }

        // 🧟 ZOMBIE
        ZombieBreak zb = hitObj.GetComponentInParent<ZombieBreak>();
        if (zb != null)
        {
            zb.Break(hitPoint, dir);
        }

        // 🥫 CAN
        if (hitObj.CompareTag("Can"))
        {
            CanState cs = hitObj.GetComponent<CanState>();
            if (cs != null)
                cs.isBroken = true;

            float duration = SpawnCanParticle(hitPoint, dir);
            PlaySound(canHitClip);

            StartCoroutine(DisableAfterEffect(hitObj, duration));
        }

        // 💪 FORCE
        Rigidbody hitRb = hitObj.GetComponent<Rigidbody>();
        if (hitRb != null)
        {
            hitRb.AddForce(dir * 5f, ForceMode.Impulse);
        }

        // 🎯 REMOVE FROM TARGET LIST
        if (target != null && shooter != null)
        {
            shooter.RemoveTarget(target);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Obstacle")) return;

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 dir = rb.velocity.normalized;

        BreakableBoard breakable = other.GetComponentInParent<BreakableBoard>();
        if (breakable != null)
        {
            breakable.RegisterBulletHit(hitPoint, dir);
        }

        if (target != null && shooter != null)
        {
            shooter.RemoveTarget(target);
        }

        Destroy(gameObject);
    }

    // 🎯 SET TARGET
    public void SetTarget(Transform t)
    {
        target = t;
    }

    public void SetShooter(CarShooter s)
    {
        shooter = s;
    }

    float SpawnCanParticle(Vector3 position, Vector3 direction)
    {
        if (canHitParticle == null) return 0f;

        Quaternion rot = Quaternion.LookRotation(-direction);

        ParticleSystem instance = Instantiate(canHitParticle, position, rot);
        instance.Play();

        float duration = instance.main.duration + instance.main.startLifetime.constantMax;

        Destroy(instance.gameObject, duration);

        return duration;
    }

    void CreateAudio()
    {
        GameObject audioObj = new GameObject("ImpactAudio");
        audioObj.transform.position = transform.position;

        impactAudio = audioObj.AddComponent<AudioSource>();
        impactAudio.spatialBlend = 1f;
        impactAudio.playOnAwake = false;
    }

    void PlaySound(AudioClip clip)
    {
        if (impactAudio == null || clip == null) return;

        impactAudio.pitch = Random.Range(0.9f, 1.1f);
        impactAudio.PlayOneShot(clip);

        Destroy(impactAudio.gameObject, clip.length);
    }

    System.Collections.IEnumerator DisableAfterEffect(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj != null)
            obj.SetActive(false);
    }
}