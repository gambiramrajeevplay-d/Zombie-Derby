using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarBoost : MonoBehaviour
{
    [Header("Boost Settings")]
    public float boostMultiplier = 2f;
    public float boostDuration = 2f;
    public float boostForce = 5000f;

    [Header("Boost State")]
    public bool isBoosting = false;

    [Header("Boost VFX")]
    public List<ParticleSystem> boostParticles; // 🔥 assign in inspector

    private SimpleCarController carController;
    private Rigidbody rb;

    private float originalTorque;
    private Coroutine boostCoroutine;

    void Start()
    {
        carController = GetComponent<SimpleCarController>();
        rb = GetComponent<Rigidbody>();

        originalTorque = carController.motorTorque;

        // 🔒 Ensure particles are OFF at start
        SetParticles(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boost"))
        {
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

        // 🔥 TURN ON PARTICLES
        SetParticles(true);

        // Reset torque
        carController.motorTorque = originalTorque;

        // Apply boost
        carController.motorTorque *= boostMultiplier;

        // Forward push
        rb.AddForce(transform.forward * boostForce, ForceMode.Impulse);

        yield return new WaitForSeconds(boostDuration);

        // Reset torque
        carController.motorTorque = originalTorque;

        isBoosting = false;

        // 🔥 TURN OFF PARTICLES
        SetParticles(false);

        boostCoroutine = null;
    }

    // 🔧 Helper method
    void SetParticles(bool state)
    {
        foreach (ParticleSystem ps in boostParticles)
        {
            if (ps == null) continue;

            if (state)
                ps.Play();
            else
                ps.Stop();
        }
    }
}