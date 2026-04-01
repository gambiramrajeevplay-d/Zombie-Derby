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
    public float motorTorque = 2500f;
    public float maxSpeed = 120f;
    public float brakeForce = 4500f;

    [Header("Brake Tuning")]
    public float brakeSharpness = 5f;
    public float brakeDrag = 1.5f;

    [Header("Idle Brake Tuning")]
    public float idleBrakeDelay = 1.5f;
    public float idleBrakeSharpness = 2f;

    [Header("Control")]
    public bool canControl = true;
    public bool canSteer = true;

    [Header("Stability")]
    public float stabilityForce = 6f;

    [Header("Air Behavior")]
    public float airGravityMultiplier = 0.7f;
    public float airRotationDamping = 0.995f;
    public float airForwardStability = 2f;

    [Header("Air Lift")]
    public float airLiftStrength = 0.02f;

    [Header("Low Speed Stability")]
    public float minSpeedThreshold = 0.5f;
    public float lowSpeedDrag = 2f;

    [Header("Landing Behavior")]
    public float landingSmoothingTime = 0.2f;
    private float landingTimer = 0f;
    private bool wasInAir = false;

    private Rigidbody rb;
    private Vector3 lockedForward;

    private float idleTimer = 0f;

    private float forwardInput;
    private float horizontalInput;
    private bool isBraking;

    private float currentSteer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = new Vector3(0, -0.5f, 0);
        rb.mass = 500f;

        rb.drag = 0.05f;
        rb.angularDrag = 0.5f;

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        lockedForward = transform.forward;

        SetWheelFriction(frontLeftWheel);
        SetWheelFriction(frontRightWheel);
        SetWheelFriction(rearLeftWheel);
        SetWheelFriction(rearRightWheel);
    }

    void Update()
    {
        forwardInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        isBraking = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
    }

    void FixedUpdate()
    {
        if (canControl)
            HandleInput();
        else
            StopCar();

        bool grounded = IsGrounded();

        if (grounded)
        {
            if (wasInAir)
            {
                landingTimer = landingSmoothingTime;
                wasInAir = false;
            }

            HandleLandingSmoothing();
            StabilizeCar();
            UpdateAllWheels();
        }
        else
        {
            wasInAir = true;
            HandleAirPhysics();
        }
    }

    void HandleInput()
    {
        float speed = rb.velocity.magnitude;

        // ✅ LOW SPEED JITTER FIX
        if (speed < minSpeedThreshold && Mathf.Abs(forwardInput) < 0.1f)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rearLeftWheel.motorTorque = 0;
            rearRightWheel.motorTorque = 0;

            rearLeftWheel.brakeTorque = brakeForce * 0.5f;
            rearRightWheel.brakeTorque = brakeForce * 0.5f;

            rb.drag = lowSpeedDrag;

            return;
        }

        if (Mathf.Abs(forwardInput) > 0.01f || Mathf.Abs(horizontalInput) > 0.01f || isBraking)
            idleTimer = 0f;

        if (forwardInput > 0.01f && speed * 3.6f < maxSpeed)
        {
            float torque = forwardInput * motorTorque;
            rearLeftWheel.motorTorque = torque;
            rearRightWheel.motorTorque = torque;
        }
        else
        {
            rearLeftWheel.motorTorque = 0;
            rearRightWheel.motorTorque = 0;
        }

        if (isBraking && IsGrounded())
        {
            float brake = brakeForce * Mathf.Lerp(0.7f, 1.2f, speed / 15f);

            rearLeftWheel.brakeTorque = brake;
            rearRightWheel.brakeTorque = brake;

            rb.drag = brakeDrag;
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * brakeSharpness);
        }
        else if (forwardInput <= 0.01f)
        {
            idleTimer += Time.fixedDeltaTime;

            if (idleTimer >= idleBrakeDelay)
            {
                rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * idleBrakeSharpness);

                rearLeftWheel.brakeTorque = brakeForce * 0.5f;
                rearRightWheel.brakeTorque = brakeForce * 0.5f;

                rb.drag = 0.2f;
            }
            else
            {
                rearLeftWheel.brakeTorque = 0;
                rearRightWheel.brakeTorque = 0;
                rb.drag = 0.05f;
            }
        }
        else
        {
            rearLeftWheel.brakeTorque = 0;
            rearRightWheel.brakeTorque = 0;
            rb.drag = 0.05f;
        }

        if (canSteer)
        {
            float targetSteer = horizontalInput * 20f;
            currentSteer = Mathf.Lerp(currentSteer, targetSteer, Time.fixedDeltaTime * 5f);

            frontLeftWheel.steerAngle = currentSteer;
            frontRightWheel.steerAngle = currentSteer;

            if (Mathf.Abs(horizontalInput) > 0.01f)
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

    void HandleAirPhysics()
    {
        float speed = rb.velocity.magnitude;

        rb.AddForce(Physics.gravity * (airGravityMultiplier - 1f), ForceMode.Acceleration);

        if (speed > 5f)
        {
            rb.AddForce(Vector3.up * speed * airLiftStrength, ForceMode.Acceleration);
        }

        rb.angularVelocity *= airRotationDamping;

        Vector3 forwardProjected = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        if (forwardProjected != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forwardProjected, Vector3.up);
            rb.rotation = Quaternion.Lerp(rb.rotation, targetRotation, Time.fixedDeltaTime * airForwardStability);
        }
    }

    void HandleLandingSmoothing()
    {
        if (landingTimer > 0f)
        {
            landingTimer -= Time.fixedDeltaTime;

            float t = 1f - (landingTimer / landingSmoothingTime);

            float currentGravity = Mathf.Lerp(airGravityMultiplier, 1f, t);
            rb.AddForce(Physics.gravity * (currentGravity - 1f), ForceMode.Acceleration);

            Vector3 vel = rb.velocity;
            vel.y = Mathf.Lerp(vel.y, 0f, t * 0.5f);
            rb.velocity = vel;

            rb.angularVelocity *= 0.9f;
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

        if (rb.velocity.magnitude > 1f)
            rb.angularVelocity *= 0.98f;
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
        friction.stiffness = 2.5f;
        wheel.sidewaysFriction = friction;
    }
}