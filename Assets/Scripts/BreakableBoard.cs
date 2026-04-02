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

    [Header("Tear Forces")]
    [SerializeField] private float pushForce = 2.5f;
    [SerializeField] private float upwardForce = 1.2f;
    [SerializeField] private float randomTorque = 3f;

    [Header("Disable On Break")]
    [SerializeField] private Collider[] collidersToDisable;

    [Header("Audio")]
    public AudioSource breakAudioSource;      // 🔊 Assign in Inspector
    public AudioClip breakClip;               // 🔊 Optional (recommended)

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

            PlayBreakSound();     // 🔥 PLAY AUDIO
            DisableColliders();
            BreakWall(hitPoint, hitDirection);

            StartCoroutine(DestroyAfterDelay());
        }
    }

    // 🔊 One-shot audio
    private void PlayBreakSound()
    {
        if (breakAudioSource == null) return;

        if (breakClip != null)
            breakAudioSource.PlayOneShot(breakClip);
        else
            breakAudioSource.Play(); // if clip already set on AudioSource
    }

    private void BreakWall(Vector3 hitPoint, Vector3 hitDirection)
    {
        foreach (Rigidbody rb in pieces)
        {
            if (rb == null) continue;

            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 dirFromHit = (rb.worldCenterOfMass - hitPoint).normalized;

            rb.AddForce((dirFromHit + hitDirection) * pushForce, ForceMode.Impulse);
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
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

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}


