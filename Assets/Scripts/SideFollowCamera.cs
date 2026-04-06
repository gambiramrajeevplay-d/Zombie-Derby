using UnityEngine;

public class SideFollowCamera : MonoBehaviour
{
    private Transform target;
    private Rigidbody targetRb;
    private CarBoost boostScript;

    [Header("Offset Settings")]
    public Vector3 offset = new Vector3(-8f, 4f, -6f);

    [Header("Smooth Settings")]
    public float followSpeed = 5f;

    [Header("Axis Lock")]
    public bool lockY = true;
    public bool lockZ = true;

    [Header("FOV Settings")]
    public float slowFOV = 60f;
    public float mediumFOV = 70f;
    public float boostFOV = 80f;

    public float speedThreshold = 20f;
    public float fovSmoothSpeed = 5f;

    private Camera cam;

    private Vector3 initialPosition;
    private Quaternion fixedRotation;

    void Start()
    {
        cam = GetComponent<Camera>();

        initialPosition = transform.position;
        fixedRotation = transform.rotation;

        FindCar();
    }

    void FindCar()
    {
        GameObject pivot = GameObject.Find("Camera_Pivot");

        if (pivot != null)
        {
            target = pivot.transform;

            targetRb = pivot.GetComponentInParent<Rigidbody>();
            boostScript = pivot.GetComponentInParent<CarBoost>();

            Debug.Log("✅ Camera target found: Camera_Pivot");
        }
        else
        {
            Debug.LogWarning("Camera_Pivot not found! Retrying...");
            Invoke(nameof(FindCar), 1f);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 🔥 IMPROVED FOLLOW (more stable)
        Vector3 desiredPosition = target.position + offset;

        if (lockY) desiredPosition.y = initialPosition.y;
        if (lockZ) desiredPosition.z = initialPosition.z;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);

        // 🔥 LOOK AT WITH X-AXIS LOCK (no vertical tilt)
        Vector3 direction = target.position - transform.position;
        direction.y = 0f; // 🔥 lock X rotation

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // 🔥 smoother rotation (no snapping)
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * followSpeed);
        }

        HandleFOV();
    }

    void HandleFOV()
    {
        if (cam == null || targetRb == null) return;

        float speed = targetRb.velocity.magnitude * 3.6f;

        float targetFOV = slowFOV;

        if (IsBoosting())
        {
            targetFOV = boostFOV;
        }
        else if (speed > speedThreshold)
        {
            targetFOV = mediumFOV;
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
    }

    bool IsBoosting()
    {
        return boostScript != null && boostScript.isBoosting;
    }
}