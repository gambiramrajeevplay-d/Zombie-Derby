using UnityEngine;

public class SideFollowCamera : MonoBehaviour
{
    private Transform target;
    private Rigidbody targetRb;
    private CarBoost boostScript; // 👈 ADD THIS

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
        SimpleCarController car = FindObjectOfType<SimpleCarController>();

        if (car != null)
        {
            target = car.transform;
            targetRb = car.GetComponent<Rigidbody>();
            boostScript = car.GetComponent<CarBoost>(); // 👈 IMPORTANT
        }
        else
        {
            Debug.LogWarning("No Car found! Retrying...");
            Invoke(nameof(FindCar), 1f);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = transform.position;
        desiredPosition.x = target.position.x + offset.x;

        desiredPosition.y = initialPosition.y;
        desiredPosition.z = initialPosition.z;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.rotation = fixedRotation;

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
        return boostScript != null && boostScript.isBoosting; // ✅ REAL BOOST CHECK
    }
}