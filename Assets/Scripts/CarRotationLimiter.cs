using UnityEngine;

public class CarRotationLimiter : MonoBehaviour
{
    public float maxPitchAngle = 25f;
    public float smoothSpeed = 5f;

    private RCC_CarControllerV3 car;
    private Rigidbody rb;

    void Start()
    {
        car = GetComponent<RCC_CarControllerV3>();
        rb = car.rigid; // ✅ use RCC rigidbody
    }

    void FixedUpdate()
    {
        LimitXRotation();
    }

    void LimitXRotation()
    {
        Vector3 angles = transform.eulerAngles;

        float x = angles.x;

        // convert 0–360 to -180 to 180
        if (x > 180f)
            x -= 360f;

        // ✅ clamp X rotation
        float clampedX = Mathf.Clamp(x, -maxPitchAngle, maxPitchAngle);

        // ✅ smooth correction
        Quaternion targetRot = Quaternion.Euler(clampedX, angles.y, 0f);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.fixedDeltaTime * smoothSpeed
        );

        // ✅ damp RCC angular velocity
        Vector3 angVel = rb.angularVelocity;
        angVel.x *= 0.5f;
        rb.angularVelocity = angVel;
    }
}