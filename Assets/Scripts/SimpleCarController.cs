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
    public float motorTorque = 3500f;
    public float maxSpeed = 120f;
    public float brakeForce = 4500f;

    [Header("Brake Tuning")]
    public float brakeSharpness = 5f;
    public float brakeDrag = 1.2f;

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

        rb.centerOfMass = new Vector3(0, -0.8f, 0);
        rb.mass = 350f;

        rb.drag = 0.02f;
        rb.angularDrag = 0.5f;

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        lockedForward = transform.forward;

        rb.solverIterations = 10;
        rb.solverVelocityIterations = 10;

        SetAllWheelFriction();
    }

    void Update()
    {
        forwardInput = Input.GetAxis("Vertical"); // ONLY forward/back
        horizontalInput = 0f; // ❌ disable steering
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
            // 🔥 LANDING RESET (unchanged logic)
            if (wasInAir)
            {
                landingTimer = landingSmoothingTime;
                wasInAir = false;
            }

            // 🔥🔥🔥 GROUND NORMAL DETECTION (NEW)
            RaycastHit hit;
            Vector3 groundNormal = Vector3.up;

            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
            {
                groundNormal = hit.normal;

                // 🔥 SOFT ALIGNMENT (NO HARD SNAP)
              
                Vector3 projected = Vector3.ProjectOnPlane(rb.velocity, groundNormal);

                // 🔥 softer blending
                rb.velocity = Vector3.Lerp(rb.velocity, projected, Time.fixedDeltaTime * 6f);
            }

            // 🔥 REDUCED STICK FORCE (NO SHAKING)
          float speedFactor = Mathf.Clamp01(rb.velocity.magnitude / 10f);
float stickForce = Mathf.Lerp(2f, 10f, speedFactor);

rb.AddForce(-groundNormal * stickForce, ForceMode.Acceleration);

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

    // ONLY HandleInput() UPDATED — rest of your script unchanged

    void HandleInput()
    {
        float speed = rb.velocity.magnitude;

        if (speed < minSpeedThreshold && Mathf.Abs(forwardInput) < 0.1f)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 3f);
            rb.angularVelocity *= 0.9f;

            rearLeftWheel.motorTorque = 0;
            rearRightWheel.motorTorque = 0;

            rearLeftWheel.brakeTorque = brakeForce * 0.3f;
            rearRightWheel.brakeTorque = brakeForce * 0.3f;

            rb.drag = 0.2f;
            return;
        }
        if (Mathf.Abs(forwardInput) > 0.01f || Mathf.Abs(horizontalInput) > 0.01f || isBraking)
            idleTimer = 0f;

        // 🔥 ADVANCED SLOPE SYSTEM
        float slopeBoost = 1f;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle > 5f)
            {
                float slopeFactor = Mathf.InverseLerp(0f, 45f, slopeAngle);

                // 🔥 smooth uphill slowdown
                slopeBoost = Mathf.Lerp(1f, 0.7f, slopeFactor);
            }
        }
        // 🔥🔥🔥 UPDATED TORQUE SYSTEM (SMOOTH ENGINE FEEL)
        if (forwardInput > 0.01f)
        {
            float speedNormalized = Mathf.Clamp01(speed / (maxSpeed / 3.6f));

            // 🔥 smoother acceleration curve
            float torqueCurve = Mathf.Pow(1f - speedNormalized, 1.5f);

            float targetTorque = forwardInput * motorTorque * slopeBoost * torqueCurve;

            // 🔥 engine inertia (very important)
            float accelerationSpeed = 5f;

            rearLeftWheel.motorTorque = Mathf.Lerp(rearLeftWheel.motorTorque, targetTorque, Time.fixedDeltaTime * accelerationSpeed);
            rearRightWheel.motorTorque = Mathf.Lerp(rearRightWheel.motorTorque, targetTorque, Time.fixedDeltaTime * accelerationSpeed);
        }
        else
        {
            // 🔥 smooth torque drop (no instant stop)
            rearLeftWheel.motorTorque = Mathf.Lerp(rearLeftWheel.motorTorque, 0f, Time.fixedDeltaTime * 2f);
            rearRightWheel.motorTorque = Mathf.Lerp(rearRightWheel.motorTorque, 0f, Time.fixedDeltaTime * 2f);
        }

        // 🔥🔥🔥 IMPROVED BRAKE FEEL
        if (isBraking && IsGrounded())
        {
            float brake = brakeForce * Mathf.Clamp01(speed / 10f); // 🔥 changed

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

                rb.drag = 0.1f;
            }
            else
            {
                rearLeftWheel.brakeTorque = 0;
                rearRightWheel.brakeTorque = 0;
                rb.drag = 0.02f;
            }
        }
        else
        {
            rearLeftWheel.brakeTorque = 0;
            rearRightWheel.brakeTorque = 0;
            rb.drag = 0.02f;
        }

        // 🔥🔥🔥 NATURAL ROLLING RESISTANCE (NEW)
        if (!isBraking && Mathf.Abs(forwardInput) < 0.1f)
        {
            rb.velocity *= 0.995f;
        }

       

        // 🔥 existing speed limit (unchanged)
        float currentSpeedKmh = rb.velocity.magnitude * 3.6f;

        if (currentSpeedKmh > maxSpeed)
        {
            rb.velocity = Vector3.Lerp(
                rb.velocity,
                rb.velocity.normalized * (maxSpeed / 3.6f),
                Time.fixedDeltaTime * 5f
            );
        }
        // 🔥 lock sideways movement
        Vector3 localVel = transform.InverseTransformDirection(rb.velocity);
        localVel.x = 0f;
        rb.velocity = transform.TransformDirection(localVel);

        // 🔥 keep facing forward
        Quaternion targetRotation = Quaternion.LookRotation(lockedForward, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);

        // 🔥 ensure no steering applied
        frontLeftWheel.steerAngle = 0f;
        frontRightWheel.steerAngle = 0f;
    }

    void HandleAirPhysics() { /* unchanged */ }
    void HandleLandingSmoothing() { /* unchanged */ }

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

    void StopCar() { /* unchanged */ }

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

    void SetAllWheelFriction()
    {
        SetWheelFriction(frontLeftWheel);
        SetWheelFriction(frontRightWheel);
        SetWheelFriction(rearLeftWheel);
        SetWheelFriction(rearRightWheel);
    }

    void SetWheelFriction(WheelCollider wheel)
    {
        WheelFrictionCurve forward = wheel.forwardFriction;
        forward.stiffness = 3.5f;
        wheel.forwardFriction = forward;

        WheelFrictionCurve sideways = wheel.sidewaysFriction;
        sideways.stiffness = 3.0f;
        wheel.sidewaysFriction = sideways;
    }

    // 🔥 RCC STYLE BUTTON
    [ContextMenu("Create Wheel Colliders")]
    public void CreateWheelColliders()
    {
        CreateWheel(ref frontLeftWheel, frontLeftModel, "FL_WheelCollider");
        CreateWheel(ref frontRightWheel, frontRightModel, "FR_WheelCollider");
        CreateWheel(ref rearLeftWheel, rearLeftModel, "RL_WheelCollider");
        CreateWheel(ref rearRightWheel, rearRightModel, "RR_WheelCollider");

        Debug.Log("✅ Wheel Colliders Created");
    }

    void CreateWheel(ref WheelCollider wc, Transform model, string name)
    {
        if (model == null) return;

        if (wc != null)
            DestroyImmediate(wc.gameObject);

        GameObject go = new GameObject(name);
        go.transform.parent = transform;

        // ✅ MATCH CAR ROTATION (CRITICAL FIX)
        go.transform.rotation = transform.rotation;

        // ✅ AUTO RADIUS
        float radius = 0.35f;

        MeshFilter mf = model.GetComponent<MeshFilter>();
        if (mf != null)
        {
            radius = mf.sharedMesh.bounds.size.y * model.localScale.y * 0.5f;
        }

        // ✅ CORRECT POSITION
        Vector3 pos = model.position;
        pos.y += radius;

        go.transform.position = pos;

        wc = go.AddComponent<WheelCollider>();

        wc.radius = radius;

        // 🔥 PERFECT SUSPENSION (RCC STYLE)
        JointSpring spring = wc.suspensionSpring;
        spring.spring = 18000f;
        spring.damper = 2500f;
        spring.targetPosition = 0.5f;
        wc.suspensionSpring = spring;

        wc.suspensionDistance = 0.3f;

        // 🔥 VERY IMPORTANT (fixes getting stuck)
        wc.forceAppPointDistance = 0.45f;

        // 🔥 PROPER FRICTION (BIG FIX)
        WheelFrictionCurve forward = wc.forwardFriction;
        forward.extremumSlip = 0.4f;
        forward.extremumValue = 1.0f;
        forward.asymptoteSlip = 0.8f;
        forward.asymptoteValue = 0.9f;
        forward.stiffness = 1.4f;
        wc.forwardFriction = forward;

        WheelFrictionCurve sideways = wc.sidewaysFriction;
        sideways.extremumSlip = 0.3f;
        sideways.extremumValue = 1.0f;
        sideways.asymptoteSlip = 0.8f;
        sideways.asymptoteValue = 0.9f;
        sideways.stiffness = 1.6f;
        wc.sidewaysFriction = sideways;
        wc.wheelDampingRate = 1.5f;
    }
}