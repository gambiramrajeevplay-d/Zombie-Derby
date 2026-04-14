using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ForwardMovement : MonoBehaviour
{
    public float acceleration = 25f;
    public float maxSpeed = 20f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Apply forward force
        if (rb.velocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
        }

        // Keep car stable (no unwanted rotation)
        rb.angularVelocity = Vector3.zero;
    }
}