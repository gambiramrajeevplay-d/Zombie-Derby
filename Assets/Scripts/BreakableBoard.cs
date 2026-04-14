using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBoard : MonoBehaviour
{
    [Header("Break Settings")]
    [SerializeField] private int requiredHits = 20;
    [SerializeField] private float destroyDelay = 4f;

    private int currentHits = 0;
    private bool broken = false;

    [Header("Pieces (Pre-fractured wall parts)")]
    [SerializeField] private Rigidbody[] pieces;

    [Header("💥 Explosion Settings")]
    [SerializeField] private float explosionForce = 12f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float upwardModifier = 1.5f;

    [Header("Extra Randomness")]
    [SerializeField] private float randomForce = 2f;
    [SerializeField] private float randomTorque = 4f;

    [Header("Disable On Break")]
    [SerializeField] private Collider[] collidersToDisable;

    [Header("Audio")]
  
    public AudioClip breakClip;

    // 🔹 BACKWARD COMPATIBILITY
    public void RegisterBulletHit()
    {
        RegisterBulletHit(transform.position, transform.forward);
    }

    public void RegisterBulletHit(Vector3 hitPoint, Vector3 hitDirection)
    {
        if (broken) return;

        currentHits++;

        if (currentHits >= requiredHits)
        {
            broken = true;

            PlayBreakSound();
            DisableColliders();
            Explode(hitPoint, hitDirection);

            StartCoroutine(DestroyAfterDelay());
        }
    }

    // 💥 EXPLOSION LOGIC
    private void Explode(Vector3 hitPoint, Vector3 hitDirection)
    {
        foreach (Rigidbody rb in pieces)
        {
            if (rb == null) continue;

            rb.isKinematic = false;
            rb.useGravity = true;

            // 💥 REAL EXPLOSION FORCE
            rb.AddExplosionForce(
                explosionForce,
                hitPoint,              // explosion center
                explosionRadius,
                upwardModifier,
                ForceMode.Impulse
            );

            // 🔥 EXTRA PUSH (direction-based)
            rb.AddForce(hitDirection * randomForce, ForceMode.Impulse);

            // 🔄 RANDOM SPIN
            rb.AddTorque(Random.insideUnitSphere * randomTorque, ForceMode.Impulse);
        }
    }

    private void DisableColliders()
    {
        foreach (Collider col in collidersToDisable)
        {
            if (col != null)
                col.enabled = false;
        }
    }

    private void PlayBreakSound()
    {
        if (breakClip == null) return;

        // 🎧 Create temp audio object
        GameObject audioObj = new GameObject("BreakSound");
        audioObj.transform.position = transform.position;

        AudioSource source = audioObj.AddComponent<AudioSource>();

        // 🔊 3D sound settings
        source.spatialBlend = 1f;
        source.playOnAwake = false;
        source.pitch = Random.Range(0.9f, 1.1f);

        source.PlayOneShot(breakClip);

        // 🧹 Destroy after sound ends
        Destroy(audioObj, breakClip.length);
    }
    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}