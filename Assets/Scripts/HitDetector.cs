using UnityEngine;

public class HitDetector : MonoBehaviour
{
    public Rigidbody playerRb;

    [Header("Impact Settings")]
    public float minBreakSpeed = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Zombie")) return;

        ZombieBreak zb = other.GetComponentInParent<ZombieBreak>();
        if (zb == null) return;

        float speed = playerRb.velocity.magnitude;
        if (speed < minBreakSpeed) return;

        Vector3 impactDir = playerRb.velocity.normalized;
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        zb.Break(hitPoint, impactDir);
    }
}