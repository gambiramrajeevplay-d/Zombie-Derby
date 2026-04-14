using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarBoost : MonoBehaviour
{
    [Header("Boost Settings")]
    public float boostDuration = 2f;
    public float extraForce = 2000f;

    [Header("Boost State")]
    public bool isBoosting = false;

    [Header("Boost VFX")]
    public List<ParticleSystem> boostParticles;

    [Header("Boost Sound")]
    public AudioClip boostClip;

    private AudioSource boostAudio;

    // ✅ RCC reference
    private RCC_CarControllerV3 car;

    private Coroutine boostCoroutine;

    [Header("Boost Extra Objects")]
    public List<GameObject> boostObjects; // 🔥 assign in inspector

    void Start()
    {
        car = GetComponent<RCC_CarControllerV3>();

        SetParticles(false);
        SetBoostObjects(false);
        CreateBoostAudio();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boost"))
        {
            if (boostCoroutine != null)
                StopCoroutine(boostCoroutine);

            boostCoroutine = StartCoroutine(Boost(other.gameObject));
        }
    }

    void CreateBoostAudio()
    {
        GameObject audioObj = new GameObject("BoostAudio");
        audioObj.transform.parent = transform;
        audioObj.transform.localPosition = Vector3.zero;

        boostAudio = audioObj.AddComponent<AudioSource>();
        boostAudio.spatialBlend = 1f;
        boostAudio.playOnAwake = false;
    }

    IEnumerator Boost(GameObject pickup)
    {
        isBoosting = true;

        // 🔊 PLAY SOUND
        if (boostAudio != null && boostClip != null)
        {
            boostAudio.pitch = Random.Range(0.95f, 1.1f);
            boostAudio.volume = 1.2f;
            boostAudio.PlayOneShot(boostClip);
        }

        // 🔥 PARTICLES ON
        SetParticles(true);
        SetBoostObjects(true);

        // 🔥 HIDE PICKUP
        pickup.SetActive(false);

        // ✅ ENABLE RCC BOOST SYSTEM
        car.useNOS = true;
        car.useTurbo = true;
        car.fuelInput = 1f;

        float timer = 0f;

        while (timer < boostDuration)
        {
            // 🔥 IMPORTANT (RCC needs this)
            car.boostInput = 1f;
            car.gasInput = 1f;

            // 🔥 EXTRA PUSH (for game feel)
            car.rigid.AddForce(transform.forward * extraForce, ForceMode.Acceleration);

            timer += Time.deltaTime;
            yield return null;
        }

        // ❌ STOP BOOST
        car.boostInput = 0f;

        isBoosting = false;

        // 🔥 PARTICLES OFF
        SetParticles(false);
        SetBoostObjects(false);

        boostCoroutine = null;

        Destroy(pickup);
    }

    void SetParticles(bool state)
    {
        foreach (ParticleSystem ps in boostParticles)
        {
            if (ps == null) continue;

            if (state) ps.Play();
            else ps.Stop();
        }
    }
    void SetBoostObjects(bool state)
    {
        foreach (GameObject obj in boostObjects)
        {
            if (obj == null) continue;

            obj.SetActive(state);
        }
    }
}