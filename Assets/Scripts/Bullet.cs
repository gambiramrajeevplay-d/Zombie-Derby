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
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        CreateAudio();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = -Vector3.right * speed;

            rb.constraints = RigidbodyConstraints.FreezeRotation;

            //    rb.constraints = RigidbodyConstraints.FreezePositionY |
            //                     RigidbodyConstraints.FreezePositionZ |
            //                     RigidbodyConstraints.FreezeRotation;
        }
        //if (target != null)
        //{
        //    //Vector3 dir = (target.position - transform.position).normalized;
        //    //rb.velocity = dir * speed;
        //}
        //else
        //{
        //    rb.velocity = transform.forward * speed;
        //}
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObj = collision.gameObject;

        Vector3 hitPoint = collision.contacts[0].point;
        Vector3 dir = -Vector3.right;

        // 🧟 ZOMBIE
        ZombieBreak zb = hitObj.GetComponentInParent<ZombieBreak>();
        if (zb != null)
        {
            zb.Break(hitPoint, dir);
        }

        // 🥫 CAN (ONLY HERE WE SPAWN PARTICLES + SOUND)
        if (hitObj.CompareTag("Can"))
        {
            CanState cs = hitObj.GetComponent<CanState>();
            if (cs != null)
                cs.isBroken = true; // ✅ mark as broken

            float duration = SpawnCanParticle(hitPoint, dir);
            PlaySound(canHitClip);

            StartCoroutine(DisableAfterEffect(hitObj, duration));
        }

        // 🟫 BOX (ONLY SOUND)
        else if (hitObj.CompareTag("Obstacle"))
        {
            PlaySound(boxHitClip);
        }

        // 💪 FORCE
        Rigidbody hitRb = hitObj.GetComponent<Rigidbody>();
        if (hitRb != null)
        {
            hitRb.AddForce(dir * 5f, ForceMode.Impulse);
        }

        Destroy(gameObject);
    }
    public void SetTarget(Transform t)
    {
        target = t;
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
        audioObj.transform.parent = null;
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