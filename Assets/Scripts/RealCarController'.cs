using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RealCarController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Movement")]
    public float motorForce = 3000f;   // increased
    public float maxSpeed = 25f;

    [Header("Steering")]
    public float steerForce = 5f;
    public float steerSmoothness = 3f;

    [Header("Grip")]
    public float sideFriction = 8f;

    [Header("Ground Check")]
    public float groundDistance = 0.8f;
    public LayerMask groundLayer;

    [Header("Stability")]
    public float downforce = 50f;

    private float moveInput;
    private float steerInput;
    private float currentSteer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.mass = 1200f;
        rb.drag = 0.05f;   // reduced drag
        rb.angularDrag = 3f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        bool grounded = Physics.Raycast(transform.position, Vector3.down, groundDistance, groundLayer);

        if (grounded)
        {
            ApplyMovement();
            ApplySteering();
            ApplyGrip();
            ApplyDownforce();
        }
    }

    void ApplyMovement()
    {
        float speed = Vector3.Dot(rb.velocity, transform.forward);

        if (moveInput > 0)
        {
            if (speed < maxSpeed)
                rb.AddForce(transform.forward * moveInput * motorForce, ForceMode.Acceleration);
        }
        else if (moveInput < 0)
        {
            rb.AddForce(transform.forward * moveInput * motorForce * 0.5f, ForceMode.Acceleration);
        }

        // Smooth idle slow down
        if (Mathf.Abs(moveInput) < 0.1f)
        {
            Vector3 vel = rb.velocity;
            vel.x *= 0.99f;
            vel.z *= 0.99f;
            rb.velocity = vel;
        }
    }

    void ApplySteering()
    {
        currentSteer = Mathf.Lerp(currentSteer, steerInput, Time.fixedDeltaTime * steerSmoothness);

        float turn = currentSteer * steerForce * rb.velocity.magnitude;

        Quaternion turnRotation = Quaternion.Euler(0f, turn * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    void ApplyGrip()
    {
        Vector3 localVel = transform.InverseTransformDirection(rb.velocity);
        localVel.x *= 1f / (1f + sideFriction * Time.fixedDeltaTime);
        rb.velocity = transform.TransformDirection(localVel);
    }

    void ApplyDownforce()
    {
        rb.AddForce(-transform.up * downforce * rb.velocity.magnitude);
    }
}