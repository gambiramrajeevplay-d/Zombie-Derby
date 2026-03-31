using UnityEngine;
using System.Collections;

public class CarBoost : MonoBehaviour
{
    public float boostMultiplier = 2f;     // speed increase
    public float boostDuration = 2f;       // how long boost lasts
    public float boostForce = 5000f;       // extra push

    private SimpleCarController carController;
    private Rigidbody rb;

    private float originalTorque;
    private bool isBoosting = false;

    void Start()
    {
        carController = GetComponent<SimpleCarController>();
        rb = GetComponent<Rigidbody>();

        originalTorque = carController.motorTorque;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boost") && !isBoosting)
        {
            StartCoroutine(Boost());
        }
    }

    IEnumerator Boost()
    {
        isBoosting = true;

        // 🚀 Increase power
        carController.motorTorque *= boostMultiplier;

        // 🚀 Instant forward push
        rb.AddForce(transform.forward * boostForce, ForceMode.Impulse);

        yield return new WaitForSeconds(boostDuration);

        // 🔙 Reset
        carController.motorTorque = originalTorque;

        isBoosting = false;
    }
}