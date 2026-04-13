using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostZone : MonoBehaviour
{
    public float boostDuration = 2f;

    private void OnTriggerEnter(Collider other)
    {
        RCC_CarControllerV3 car = other.GetComponent<RCC_CarControllerV3>();

        if (car != null)
        {
            StartCoroutine(ActivateBoost(car));
        }
    }

    IEnumerator ActivateBoost(RCC_CarControllerV3 car)
    {
        // Enable NOS + Turbo
        car.useNOS = true;
        car.useTurbo = true;

        // Activate boost input
        car.boostInput = 1f;
        car.rigid.AddForce(car.transform.forward * 5000f);
        yield return new WaitForSeconds(boostDuration);

        // Disable boost
        car.boostInput = 0f;
    }
}

