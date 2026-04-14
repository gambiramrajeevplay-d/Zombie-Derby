using UnityEngine;

public class SideFollowCamera : MonoBehaviour
{
    private Transform target;
    private Rigidbody targetRb;
    private CarBoost boostScript;

    [Header("Offset Settings")]
    public Vector3 offset = new Vector3(-8f, 4f, -6f);
    private Vector3 velocity = Vector3.zero;

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

        // ✅ Stable follow using SmoothDamp (better than Lerp)
        Vector3 desiredPosition = target.position + offset;

        if (lockY) desiredPosition.y = initialPosition.y;
        if (lockZ) desiredPosition.z = initialPosition.z;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            0.15f // smooth time (lower = tighter)
        );

        // ✅ Stable horizontal look (no shaking)
        Vector3 flatTarget = target.position;
        flatTarget.y = transform.position.y;

        Quaternion targetRotation = Quaternion.LookRotation(flatTarget - transform.position);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * 5f
        );

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