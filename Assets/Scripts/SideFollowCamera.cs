using UnityEngine;

public class SideFollowCamera : MonoBehaviour
{
    private Transform target;

    [Header("Offset Settings")]
    public Vector3 offset = new Vector3(-8f, 4f, -6f);

    [Header("Smooth Settings")]
    public float followSpeed = 5f;

    [Header("Axis Lock (Optional)")]
    public bool lockY = false;
    public bool lockZ = false;

    private Vector3 initialPosition;
    private Quaternion fixedRotation;

    void Start()
    {
        initialPosition = transform.position;

        // 🔒 Store fixed rotation
        fixedRotation = transform.rotation;

        FindCar();
    }

    void FindCar()
    {
        SimpleCarController car = FindObjectOfType<SimpleCarController>();

       if (car != null)
       {
           target = car.transform;
       }
        else
       {
           Debug.LogWarning("No RCC Car found! Retrying...");
           Invoke(nameof(FindCar), 1f);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 🎯 Position follow
        Vector3 desiredPosition = target.position + offset;

        // 🔒 Axis lock
        if (lockY)
            desiredPosition.y = initialPosition.y;

        if (lockZ)
            desiredPosition.z = initialPosition.z;

        // 🎥 Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // 🔒 Keep rotation FIXED
        transform.rotation = fixedRotation;
    }
}