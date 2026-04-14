using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RCC_PathStability : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform pathCenter; // center of road
    public float correctionForce = 20f;   // reduced for RCC
    public float maxCorrection = 3f;

    [Header("Stability")]
    public float rotationStability = 3f;

    [Header("Drift Control")]
    public float minSpeedToCorrect = 0.5f;
    public float zLockStrength = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (pathCenter == null) return;

        KeepCarCentered();
        StabilizeRotation();
        KillSideDrift();
    }

    void KeepCarCentered()
    {
        Vector3 pos = transform.position;
        Vector3 target = pathCenter.position;

        float offsetX = Mathf.Clamp(target.x - pos.x, -maxCorrection, maxCorrection);

        Vector3 velocity = rb.velocity;

        // ✅ Only correct if car is actually moving
        if (velocity.magnitude > minSpeedToCorrect)
        {
            velocity.x += offsetX * correctionForce * Time.fixedDeltaTime;
        }

        rb.velocity = velocity;
    }

    void KillSideDrift()
    {
        Vector3 velocity = rb.velocity;

        // 🔥 HARD LOCK Z AXIS (no sideways drift)
        velocity.z = Mathf.Lerp(velocity.z, 0f, Time.fixedDeltaTime * zLockStrength);

        // 🔥 STOP tiny unwanted movement
        if (velocity.magnitude < 0.2f)
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }

        rb.velocity = velocity;
    }

    void StabilizeRotation()
    {
        Vector3 euler = transform.eulerAngles;

        float x = NormalizeAngle(euler.x);
        float z = NormalizeAngle(euler.z);

        Vector3 torque = new Vector3(-x, 0f, -z) * rotationStability;

        rb.AddRelativeTorque(torque, ForceMode.Acceleration);
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }
}