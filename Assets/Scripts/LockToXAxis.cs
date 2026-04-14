using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LockToXAxis : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 vel = rb.velocity;

        // Allow only X movement
        vel.z = 0f;

        rb.velocity = vel;

        // Optional: lock position completely on Z
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }
}