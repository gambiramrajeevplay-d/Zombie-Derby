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
    public float motorTorque = 500f;
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

    [Header("Lane Centering")]
    public LayerMask roadLayer;     // assign your road layer
    public float rayDistance = 2.5f;
    public float centeringForce = 10f;
    public float maxOffset = 1.5f;

    private Rigidbody rb;
    private Vector3 lockedForward;

    private float idleTimer = 0f;

    private float forwardInput;
    private float horizontalInput;
    private bool isBraking;

    private float currentSteer;

    [Header("Engine Sound")]
    public bool canPlayEngineSound = true;
    public AudioClip engineClip;

    private AudioSource engineSource;

    [Range(0.5f, 3f)] public float minPitch = 0.8f;
    [Range(0.5f, 3f)] public float maxPitch = 2.2f;

    public float minVolume = 0.2f;
    public float maxVolume = 1f;
    float currentEngineForce = 0f;
    void Start()
    {
        CreateEngineAudio();

        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = new Vector3(0, -0.8f, 0);
        rb.mass = 350f;

        rb.drag = 0.1f;
        rb.angularDrag = 0.5f;

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        lockedForward = -Vector3.right; // ALWAYS move in -X

        rb.solverIterations = 10;
        rb.solverVelocityIterations = 10;



        SetAllWheelFriction();
    }

    void Update()
    {
        HandleEngineSound();
        forwardInput = Input.GetAxis("Vertical"); // ONLY forward/back
        horizontalInput = 0f; // ❌ disable steering
        isBraking = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

        if (Mathf.Abs(forwardInput) > 0.1f)
        {
            GameManager.Instance.ResetInputTimer();
        }

    }

    void FixedUpdate()
    {
        // ✅ allow X (pitch) + Z (roll), control Y only
        rb.angularVelocity = new Vector3(
            rb.angularVelocity.x * 0.98f,
            rb.angularVelocity.y * 0.5f,
            rb.angularVelocity.z * 0.98f
        );

        if (canControl)
            HandleInput();
        else
            StopCar();

        KeepCarCentered();

        bool grounded = IsGrounded();

        if (grounded)
        {
            // 🔥 small downward stick (prevents bounce)
            if (landingTimer <= 0f) // only after landing settles
            {
                rb.AddForce(Vector3.down * 2f, ForceMode.Acceleration);
            }
            if (wasInAir)
            {
                landingTimer = landingSmoothingTime;
                wasInAir = false;
            }

            RaycastHit hit;

            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
            {
                // 🔥 align car to slope (X rotation)
                Vector3 targetUp = hit.normal;
                Quaternion targetRot = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;

                // 🔥 slower + smoother alignment (prevents shaking)
                // 🔥 ONLY align AFTER landing settles
                if (landingTimer <= 0f)
                {
                    float alignSpeed = 2f; // slower = smoother

                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        Time.fixedDeltaTime * alignSpeed
                    );
                }
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


    void CreateEngineAudio()
    {
        GameObject engineObj = new GameObject("EngineAudio");
        engineObj.transform.parent = transform;
        engineObj.transform.localPosition = Vector3.zero;

        engineSource = engineObj.AddComponent<AudioSource>();
        engineSource.clip = engineClip;
        engineSource.loop = true;
        engineSource.playOnAwake = false;
        engineSource.spatialBlend = 1f; // 3D sound

        if (engineClip != null)
            engineSource.Play();
    }
    void HandleEngineSound()
    {
        if (!canPlayEngineSound || engineSource == null)
        {
            if (engineSource != null && engineSource.isPlaying)
                engineSource.Stop();
            return;
        }

        if (!engineSource.isPlaying && engineClip != null)
            engineSource.Play();

        float speed = rb.velocity.magnitude;

        // Normalize speed
        float speedPercent = Mathf.Clamp01(speed / (maxSpeed / 3.6f));

        // RCC-style pitch
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, speedPercent);
        engineSource.pitch = Mathf.Lerp(engineSource.pitch, targetPitch, Time.deltaTime * 5f);

        // Volume based on throttle + speed
        float targetVolume = Mathf.Lerp(minVolume, maxVolume, speedPercent);
        engineSource.volume = Mathf.Lerp(engineSource.volume, targetVolume, Time.deltaTime * 5f);
    }
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

        // 🔥 SLOPE BASED MOVEMENT (FIXED)
        RaycastHit hit;
        Vector3 moveDir = lockedForward;

        float slopeBoost = 1.2f;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            // Project forward direction onto slope
            moveDir = Vector3.ProjectOnPlane(lockedForward, hit.normal).normalized;

            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            // More boost on steep slopes
            slopeBoost = Mathf.Lerp(1f, 1.8f, slopeAngle / 45f);
        }

        // 🔥 APPLY FORCE INSTEAD OF ONLY TORQUE
        if (forwardInput > 0.01f)
        {
            float targetForce = motorTorque * slopeBoost * forwardInput;

            float accelRate = rb.velocity.magnitude < 2f ? 8f : 3f;

            currentEngineForce = Mathf.Lerp(currentEngineForce, targetForce, Time.fixedDeltaTime * accelRate);

            float minForce = 50f;
            float finalForce = Mathf.Max(currentEngineForce, minForce);

            rb.AddForce(moveDir * finalForce, ForceMode.Force);

            rearLeftWheel.motorTorque = motorTorque * forwardInput;
            rearRightWheel.motorTorque = motorTorque * forwardInput;

            // 🔥 IMPORTANT
            rearLeftWheel.brakeTorque = 0f;
            rearRightWheel.brakeTorque = 0f;
        }
        // 🔥 BRAKE SYSTEM (FIXED)
        if (isBraking && IsGrounded())
        {
            rearLeftWheel.brakeTorque = brakeForce;
            rearRightWheel.brakeTorque = brakeForce;

            // extra slowdown
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 3f);

            // cancel engine force while braking
            currentEngineForce = 0f;
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
        //Vector3 localVel = transform.InverseTransformDirection(rb.velocity);
        //localVel.x = 0f;
        //rb.velocity = transform.TransformDirection(localVel);

        

        // 🔥 keep facing forward
        Quaternion targetRotation = Quaternion.LookRotation(lockedForward, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);

        // 🔥 ensure no steering applied
        frontLeftWheel.steerAngle = 0f;
        frontRightWheel.steerAngle = 0f;
    }

    void HandleAirPhysics()
    {
        // 🔥 stronger gravity for better fall
        rb.AddForce(Vector3.down * airGravityMultiplier, ForceMode.Acceleration);

        // 🔥 forward stability (prevents backflip madness)
        Vector3 forwardTorque = Vector3.Cross(transform.forward, Vector3.forward);
        rb.AddTorque(forwardTorque * airForwardStability, ForceMode.Acceleration);

        // 🔥 damping (smooth rotation)
        rb.angularVelocity *= airRotationDamping;
    }
    void HandleLandingSmoothing()
    {
        if (landingTimer > 0f)
        {
            landingTimer -= Time.fixedDeltaTime;

            // ❌ REMOVE vertical force override (causes jitter)
            // rb.velocity = Vector3.Lerp(...)

            // ✅ smooth only rotation, NOT velocity
            rb.angularVelocity = Vector3.Lerp(
                rb.angularVelocity,
                Vector3.zero,
                Time.fixedDeltaTime * 3f
            );

            // ✅ small damping instead of hard force
            rb.drag = Mathf.Lerp(rb.drag, 0.3f, Time.fixedDeltaTime * 2f);
        }
        else
        {
            // reset drag after landing
            rb.drag = 0.1f;
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
        // 🔥 reduce stabilization right after landing
        float stabilityMultiplier = landingTimer > 0f ? 0.1f : 1f;

        Vector3 euler = transform.eulerAngles;

        float x = NormalizeAngle(euler.x);
        float z = NormalizeAngle(euler.z);

        Vector3 torque = new Vector3(-x, 0f, -z) * stabilityForce * stabilityMultiplier;

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
        forward.stiffness = 2.2f;
        wheel.forwardFriction = forward;

        WheelFrictionCurve sideways = wheel.sidewaysFriction;
        sideways.stiffness = 2.5f;
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
    void KeepCarCentered()
    {
        Vector3 leftOrigin = transform.position - transform.right * 0.5f;
        Vector3 rightOrigin = transform.position + transform.right * 0.5f;

        RaycastHit leftHit, rightHit;

        bool hitLeft = Physics.Raycast(leftOrigin, -transform.right, out leftHit, rayDistance, roadLayer);
        bool hitRight = Physics.Raycast(rightOrigin, transform.right, out rightHit, rayDistance, roadLayer);

        if (hitLeft && hitRight)
        {
            float leftDist = leftHit.distance;
            float rightDist = rightHit.distance;

            float offset = rightDist - leftDist;

            // clamp to avoid over-correction
            offset = Mathf.Clamp(offset, -maxOffset, maxOffset);

            // apply smooth centering force
            rb.AddForce(transform.right * offset * centeringForce, ForceMode.Acceleration);
        }
    }
}