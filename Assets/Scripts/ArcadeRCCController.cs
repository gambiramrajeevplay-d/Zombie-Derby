using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(RCC_CarControllerV3))]
public class ArcadeRCCController : MonoBehaviour
{
    private RCC_CarControllerV3 car;
    private Rigidbody rb;

    [Header("Arcade Settings")]
    public float forwardSpeed = 1f;     // always forward
    public float steerSensitivity = 1f;

    [Header("Air Control")]
    public float airRotationForce = 200f;
    public float groundCheckDistance = 1.5f;

    [Header("Boost")]
    public float boostForce = 5000f;
    public float boostDuration = 2f;

    private bool isBoosting = false;

    void Start()
    {
        car = GetComponent<RCC_CarControllerV3>();
        rb = GetComponent<Rigidbody>();

        // 🔒 Stability (IMPORTANT)
        rb.constraints = RigidbodyConstraints.FreezeRotationZ;
        rb.constraints = RigidbodyConstraints.FreezePositionZ;
        // 🔥 Enable arcade-friendly RCC features
        car.useNOS = true;
        car.useTurbo = true;

        car.ABS = true;
        car.ESP = true;
        car.TCS = true;
        car.steeringHelper = true;
        car.tractionHelper = true;

        // Reduce realism
        car.gearShiftingDelay = 0f;
        car.clutchInertia = 0.05f;

        // Strong grip & stability
        car.downForce = 150f;
       // car.angularDrag = 4f;
    }

    void Update()
    {
        HandleDriving();
    }

    void FixedUpdate()
    {
        HandleAirControl();
    }

    // 🚗 Always forward + simple steering
    void HandleDriving()
    {
        car.gasInput = forwardSpeed;

        float steer = Input.GetAxis("Horizontal");
        car.steerInput = steer * steerSensitivity;

        car.brakeInput = 0f;

        // Optional reverse
        if (Input.GetKey(KeyCode.S))
        {
            car.gasInput = 0f;
            car.brakeInput = 1f;
        }
    }

    // 🛫 Air rotation like Zombie Derby
    void HandleAirControl()
    {
        if (!IsGrounded())
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            rb.AddTorque(Vector3.up * horizontal * airRotationForce, ForceMode.Acceleration);
            rb.AddTorque(Vector3.right * vertical * airRotationForce, ForceMode.Acceleration);
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }

    // ⚡ BOOST FUNCTION (call from trigger)
    public void ActivateBoost()
    {
        if (!isBoosting)
            StartCoroutine(BoostCoroutine());
    }

    System.Collections.IEnumerator BoostCoroutine()
    {
        isBoosting = true;

        // Activate NOS
        car.boostInput = 1f;

        // Extra push
        rb.AddForce(transform.forward * boostForce, ForceMode.Impulse);

        yield return new WaitForSeconds(boostDuration);

        car.boostInput = 0f;

        isBoosting = false;
    }
}