using UnityEngine;

public class WheelColliderCreator : MonoBehaviour
{
    public Camera cam;
    public SimpleCarController carController;

    public float wheelRadius = 0.35f;

    private int wheelIndex = 0; // 🔥 controls assignment order

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SpawnWheel();
        }
    }

    void SpawnWheel()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject wcObj = new GameObject("WheelCollider_" + wheelIndex);
            wcObj.transform.parent = transform;

            wcObj.transform.position = hit.point + Vector3.up * wheelRadius;

            WheelCollider wc = wcObj.AddComponent<WheelCollider>();

            SetupWheel(wc);

            AssignWheel(wc);
        }
    }

    void AssignWheel(WheelCollider wc)
    {
        switch (wheelIndex)
        {
            case 0:
                carController.frontLeftWheel = wc;
                Debug.Log("Assigned Front Left");
                break;

            case 1:
                carController.frontRightWheel = wc;
                Debug.Log("Assigned Front Right");
                break;

            case 2:
                carController.rearLeftWheel = wc;
                Debug.Log("Assigned Rear Left");
                break;

            case 3:
                carController.rearRightWheel = wc;
                Debug.Log("Assigned Rear Right");
                break;

            default:
                Debug.Log("All wheels already assigned!");
                return;
        }

        wheelIndex++;
    }

    void SetupWheel(WheelCollider wheel)
    {
        wheel.radius = wheelRadius;

        // 🔥 RCC SUSPENSION
        JointSpring spring = wheel.suspensionSpring;
        spring.spring = 35000f;
        spring.damper = 4500f;
        spring.targetPosition = 0.5f;
        wheel.suspensionSpring = spring;

        wheel.suspensionDistance = 0.2f;
        wheel.forceAppPointDistance = 0.3f;

        // 🔥 RCC FRICTION
        WheelFrictionCurve forward = wheel.forwardFriction;
        forward.extremumSlip = 0.4f;
        forward.extremumValue = 1.2f;
        forward.asymptoteSlip = 0.8f;
        forward.asymptoteValue = 0.9f;
        forward.stiffness = 2.2f;
        wheel.forwardFriction = forward;

        WheelFrictionCurve sideways = wheel.sidewaysFriction;
        sideways.extremumSlip = 0.3f;
        sideways.extremumValue = 1.2f;
        sideways.asymptoteSlip = 0.6f;
        sideways.asymptoteValue = 0.9f;
        sideways.stiffness = 2.5f;
        wheel.sidewaysFriction = sideways;
    }
}