using UnityEngine;

public class CanImpact : MonoBehaviour
{
    [Header("Particles")]
    public ParticleSystem[] hitParticles;

    [Header("Sound")]
    public AudioClip hitSound;

    [Header("Force")]
    public float hitForce = 5f;

    private AudioSource audioSource;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        CreateAudioSource();
    }

    void CreateAudioSource()
    {
        GameObject audioObj = new GameObject("CanAudio");
        audioObj.transform.parent = transform;
        audioObj.transform.localPosition = Vector3.zero;

        audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

    public void Hit(Vector3 hitPoint, Vector3 direction)
    {
        // 🔊 PLAY SOUND
        if (audioSource != null && hitSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(hitSound);
        }

        // 💥 SPAWN PARTICLE AT EXACT HIT POINT
        if (hitParticles != null && hitParticles.Length > 0)
        {
            ParticleSystem selected = hitParticles[Random.Range(0, hitParticles.Length)];

            if (selected != null)
            {
                // 🔥 better rotation
                Quaternion rot = Quaternion.LookRotation(-direction);

                ParticleSystem instance = Instantiate(selected, hitPoint, rot);
                instance.Play();

                Destroy(instance.gameObject,
                    instance.main.duration + instance.main.startLifetime.constantMax);
            }
        }

        // 💪 APPLY FORCE
        if (rb != null)
        {
            rb.AddForce(direction * hitForce, ForceMode.Impulse);
        }
    }
}