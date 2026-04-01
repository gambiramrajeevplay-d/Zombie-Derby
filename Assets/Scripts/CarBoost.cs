using UnityEngine;
using System.Collections;

public class CarBoost : MonoBehaviour
{
    [Header("Boost Settings")]
    public float boostMultiplier = 2f;     // speed increase
    public float boostDuration = 2f;       // how long boost lasts
    public float boostForce = 5000f;       // extra push

    [Header("Boost State (Read Only)")]
    public bool isBoosting = false; // 👈 NOW PUBLIC (camera can access)

    private SimpleCarController carController;
    private Rigidbody rb;

    private float originalTorque;
    private Coroutine boostCoroutine;

    void Start()
    {
        carController = GetComponent<SimpleCarController>();
        rb = GetComponent<Rigidbody>();

        originalTorque = carController.motorTorque;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boost"))
        {
            // 🔥 Restart boost if already boosting (no stacking issues)
            if (boostCoroutine != null)
            {
                StopCoroutine(boostCoroutine);
            }

            boostCoroutine = StartCoroutine(Boost());
        }
    }

    IEnumerator Boost()
    {
        isBoosting = true;

        // 🚀 Reset to original before applying boost (prevents stacking bug)
        carController.motorTorque = originalTorque;

        // 🚀 Apply boost
        carController.motorTorque *= boostMultiplier;

        // 🚀 Instant forward push
        rb.AddForce(transform.forward * boostForce, ForceMode.Impulse);

        yield return new WaitForSeconds(boostDuration);

        // 🔙 Reset back cleanly
        carController.motorTorque = originalTorque;

        isBoosting = false;
        boostCoroutine = null;
    }
}