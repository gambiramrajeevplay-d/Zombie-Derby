using UnityEngine;

public class ObstaclePush : MonoBehaviour
{
    public float pushForce = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Obstacle")) return;

        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        // Direction from player to obstacle
        Vector3 hitDir = (collision.transform.position - transform.position).normalized;

        // Apply instant impulse (NO coroutine = NO lag)
        rb.AddForce(hitDir * pushForce, ForceMode.Impulse);
    }
}