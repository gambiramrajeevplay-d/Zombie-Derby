using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RCC_LaneCentering : MonoBehaviour
{
    public float rayDistance = 5f;
    public float correctionForce = 50f;
    public float maxCorrection = 5f;

    public LayerMask rayMask; // set this to ONLY road boundaries

    public Transform rayOrigin; // assign car center (or empty object)

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (!rayOrigin)
            rayOrigin = transform;
    }

    void FixedUpdate()
    {
        RaycastHit leftHit, rightHit;

        bool hitLeft = Physics.Raycast(rayOrigin.position, -transform.right, out leftHit, rayDistance, rayMask);
        bool hitRight = Physics.Raycast(rayOrigin.position, transform.right, out rightHit, rayDistance, rayMask);

        Debug.DrawRay(rayOrigin.position, -transform.right * rayDistance, Color.red);
        Debug.DrawRay(rayOrigin.position, transform.right * rayDistance, Color.blue);

        if (hitLeft && hitRight)
        {
            float leftDist = leftHit.distance;
            float rightDist = rightHit.distance;

            float offset = rightDist - leftDist;

            float correction = Mathf.Clamp(offset, -maxCorrection, maxCorrection);

            // Apply force sideways to center the car
            rb.AddForce(transform.right * correction * correctionForce, ForceMode.Acceleration);
        }
    }
}