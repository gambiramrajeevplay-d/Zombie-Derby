using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 80f;
    public float lifeTime = 3f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;

            // ✅ FORCE STRICT -X DIRECTION
            rb.velocity = -Vector3.right * speed;

            rb.constraints = RigidbodyConstraints.FreezePositionY |
                             RigidbodyConstraints.FreezePositionZ |
                             RigidbodyConstraints.FreezeRotation;
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObj = collision.gameObject;

        ZombieBreak zb = hitObj.GetComponentInParent<ZombieBreak>();
        if (zb != null)
        {
            zb.Break(transform.position, -Vector3.right);
        }

        if (hitObj.CompareTag("Obstacle") || hitObj.CompareTag("Can")|| hitObj.CompareTag("Zombie"))
        {
            Rigidbody hitRb = hitObj.GetComponent<Rigidbody>();

            if (hitRb != null)
            {
                hitRb.AddForce(-Vector3.right * 5f, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}