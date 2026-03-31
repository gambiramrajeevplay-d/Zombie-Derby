using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Wheel Models")]
    public Transform frontLeftModel;
    public Transform frontRightModel;
    public Transform rearLeftModel;
    public Transform rearRightModel;

    [Header("Movement")]
    public float motorTorque = 2500f; // 🔽 reduced for stability
    public float maxSpeed = 120f;
    public float brakeForce = 4000f;

    [Header("Control")]
    public bool canControl = true;
    public bool canSteer = true;

    [Header("Stability")]
    public float downForce = 15f; // 🔽 less physics cost
    public float stabilityForce = 4f;

    private Rigidbody rb;
    private Vector3 lockedForward;

    // 🔥 wheel update optimization
    private float wheelUpdateTimer = 0f;
    private float wheelUpdateRate = 0.05f; // update every 0.05 sec

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = new Vector3(0, -0.5f, 0);
        rb.mass = 500f;
        rb.drag = 0.05f;
        rb.angularDrag = 0.2f;

        // 🔥 IMPORTANT FOR MOBILE PERFORMANCE
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        lockedForward = transform.forward;

        SetWheelFriction(frontLeftWheel);
        SetWheelFriction(frontRightWheel);
        SetWheelFriction(rearLeftWheel);
        SetWheelFriction(rearRightWheel);
    }

    void FixedUpdate()
    {
        if (canControl)
            HandleInput();
        else
            StopCar();

        if (IsGrounded())
        {
            ApplyDownforce();
            StabilizeCar();
        }
        else
        {
            rb.angularVelocity *= 0.98f;
        }

        // 🔥 OPTIMIZED wheel update
        wheelUpdateTimer += Time.fixedDeltaTime;
        if (wheelUpdateTimer >= wheelUpdateRate)
        {
            UpdateAllWheels();
            wheelUpdateTimer = 0f;
        }
    }

    void HandleInput()
    {
        float forwardInput = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        bool isBraking = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        float speed = rb.velocity.magnitude * 3.6f;

        // 🚗 Forward
        if (forwardInput > 0.01f)
        {
            if (speed < maxSpeed)
            {
                rearLeftWheel.motorTorque = forwardInput * motorTorque;
                rearRightWheel.motorTorque = forwardInput * motorTorque;
            }

            rb.AddForce(transform.forward * forwardInput * 40f); // 🔽 reduced
        }
        else
        {
            rearLeftWheel.motorTorque = 0;
            rearRightWheel.motorTorque = 0;
        }

        // 🛑 Brake only grounded
        if (isBraking && IsGrounded())
        {
            rearLeftWheel.brakeTorque = brakeForce;
            rearRightWheel.brakeTorque = brakeForce;
        }
        else
        {
            rearLeftWheel.brakeTorque = 0;
            rearRightWheel.brakeTorque = 0;
        }

        // 🔄 Steering
        if (canSteer)
        {
            float steer = horizontal * 20f;
            frontLeftWheel.steerAngle = steer;
            frontRightWheel.steerAngle = steer;

            if (Mathf.Abs(horizontal) > 0.01f)
                lockedForward = transform.forward;
        }
        else
        {
            frontLeftWheel.steerAngle = 0f;
            frontRightWheel.steerAngle = 0f;

            Vector3 localVel = transform.InverseTransformDirection(rb.velocity);
            localVel.x = 0;
            rb.velocity = transform.TransformDirection(localVel);

            Quaternion targetRotation = Quaternion.LookRotation(lockedForward, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
        }
    }

    bool IsGrounded()
    {
        return frontLeftWheel.isGrounded ||
               frontRightWheel.isGrounded ||
               rearLeftWheel.isGrounded ||
               rearRightWheel.isGrounded;
    }

    void StabilizeCar()
    {
        Vector3 euler = transform.eulerAngles;

        float x = NormalizeAngle(euler.x);
        float z = NormalizeAngle(euler.z);

        Vector3 torque = new Vector3(-x, 0f, -z) * stabilityForce;
        rb.AddRelativeTorque(torque, ForceMode.Acceleration);
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }

    void StopCar()
    {
        rearLeftWheel.motorTorque = 0;
        rearRightWheel.motorTorque = 0;

        rearLeftWheel.brakeTorque = brakeForce;
        rearRightWheel.brakeTorque = brakeForce;

        frontLeftWheel.steerAngle = 0f;
        frontRightWheel.steerAngle = 0f;
    }

    void ApplyDownforce()
    {
        rb.AddForce(-transform.up * downForce * rb.velocity.magnitude);
    }

    void UpdateAllWheels()
    {
        UpdateWheel(frontLeftWheel, frontLeftModel);
        UpdateWheel(frontRightWheel, frontRightModel);
        UpdateWheel(rearLeftWheel, rearLeftModel);
        UpdateWheel(rearRightWheel, rearRightModel);
    }

    void UpdateWheel(WheelCollider col, Transform model)
    {
        if (model == null || col == null) return;

        Vector3 pos;
        Quaternion rot;
        col.GetWorldPose(out pos, out rot);

        model.position = pos;
        model.rotation = rot;
    }

    void SetWheelFriction(WheelCollider wheel)
    {
        WheelFrictionCurve friction = wheel.sidewaysFriction;
        friction.stiffness = 2f; // slightly lower for performance
        wheel.sidewaysFriction = friction;
    }
}