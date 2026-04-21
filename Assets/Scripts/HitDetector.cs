using UnityEngine;
using System.Collections;

public class HitDetector : MonoBehaviour
{
    public Rigidbody playerRb;

    [Header("Impact Settings")]
    public float minBreakSpeed = 5f;

    [Header("Impact Sounds")]
    public AudioClip canHitClip;
    public AudioClip boxHitClip;

    private AudioSource impactAudio;

    [Header("Impact Particles")]
    public ParticleSystem hitParticlePrefab;

    void Start()
    {
        CreateAudio();
    }

    private void OnTriggerEnter(Collider other)
    {
        float speed = playerRb.velocity.magnitude;

        Vector3 impactDir = playerRb.velocity.normalized;
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        // 💥 BREAKABLE
        BreakableBoard breakable = other.GetComponentInParent<BreakableBoard>();
        if (breakable != null)
        {
            if (speed >= minBreakSpeed)
            {
                breakable.RegisterBulletHit(hitPoint, impactDir);
            }
        }
        // 🧟 ZOMBIE
        if (other.CompareTag("Zombie"))
        {
            ZombieBreak zb = other.GetComponentInParent<ZombieBreak>();
            if (zb == null) return;

            if (speed < minBreakSpeed) return;

            zb.Break(hitPoint, impactDir);
            return;
        }

        // 🥫 CAN (ONLY HERE)
        if (other.CompareTag("Can"))
        {
            CanState cs = other.GetComponent<CanState>();

            // ❌ skip damage if already broken
            if (cs != null && cs.isBroken)
                return;

            PlayImpactSound(canHitClip);

            float duration = SpawnImpactParticle(hitPoint, impactDir);

            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.AddForce(impactDir * 5f, ForceMode.Impulse);
            }

            StartCoroutine(DisableAfterEffect(other.gameObject, duration));
            return;
        }

        // 🟫 BOX
        if (other.CompareTag("Obstacle"))
        {
            PlayImpactSound(boxHitClip);

            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.AddForce(impactDir * 5f, ForceMode.Impulse);
            }
        }
    }

    float SpawnImpactParticle(Vector3 position, Vector3 direction)
    {
        if (hitParticlePrefab == null) return 0.2f;

        Quaternion rot = Quaternion.LookRotation(-direction);

        ParticleSystem instance = Instantiate(hitParticlePrefab, position, rot);
        instance.Play();

        float duration = instance.main.duration + instance.main.startLifetime.constantMax;

        Destroy(instance.gameObject, duration);

        return duration;
    }

    IEnumerator DisableAfterEffect(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj != null)
            obj.SetActive(false);
    }

    void CreateAudio()
    {
        GameObject audioObj = new GameObject("ImpactAudio");
        audioObj.transform.parent = transform;
        audioObj.transform.localPosition = Vector3.zero;

        impactAudio = audioObj.AddComponent<AudioSource>();
        impactAudio.spatialBlend = 1f;
        impactAudio.playOnAwake = false;
    }

    void PlayImpactSound(AudioClip clip)
    {
        if (clip == null) return;

        GameObject temp = new GameObject("TempImpactAudio");
        temp.transform.position = transform.position;

        AudioSource a = temp.AddComponent<AudioSource>();
        a.spatialBlend = 1f;
        a.pitch = Random.Range(0.9f, 1.1f);
        a.PlayOneShot(clip);

        Destroy(temp, clip.length);
    }
}