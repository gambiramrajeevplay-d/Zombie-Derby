using UnityEngine;

public class ReverseFail : MonoBehaviour
{
    private RCC_CarControllerV3 car;

    private float reverseTimer = 0f;
    public float failTime = 5f;

    void Start()
    {
        car = GetComponent<RCC_CarControllerV3>();
    }

    void Update()
    {
        // 🚗 Check if car is reversing
        if (car.direction == -1)
        {
            reverseTimer += Time.deltaTime;

            if (reverseTimer >= failTime)
            {
                GameManager.Instance.FailLevel();
            }
        }
        else
        {
            // 🔄 Reset when not reversing
            reverseTimer = 0f;
        }
    }
}