using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarShooter : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Ammo")]
    public int maxAmmo = 10;
    private int currentAmmo;

    [Header("Fire Rate")]
    public float fireRate = 0.2f;
    private float lastShootTime;

    [Header("Auto Shoot Settings")]
    public float shootRange = 20f;
    public LayerMask targetLayers;

    [Header("Ammo UI (Top Counter)")]
    public TextMeshProUGUI ammoCounterText;

    [Header("Floating Out Of Ammo Text")]
    public FloatingText ammoFloatingText;

    [Header("Control")]
    public bool canShoot = true;

    private bool permanentlyDisabled = false;
    private bool isShowingAmmoText = false;

    [Header("Shoot Sound")]
    public AudioClip shootClip;

    private AudioSource soundSource;

    [Header("Cone Detection")]
    public float detectRadius = 1.5f;
    private List<Transform> targetList = new List<Transform>();
    private int currentIndex = 0;

    Transform currentTarget;

    float detectTimer = 0f;
    public float detectInterval = 0.15f; // 🔥 tweak (0.1–0.2 best)

    Transform lockedTarget;
    void Start()
    {
        currentAmmo = maxAmmo;

        // 🔍 Get Ammo Counter (Top UI)
        GameObject ammoCountObj = GameObject.FindGameObjectWithTag("ammo count");
        if (ammoCountObj != null)
            ammoCounterText = ammoCountObj.GetComponent<TextMeshProUGUI>();
        else
            Debug.LogWarning("No GameObject with tag 'ammo count' found!");

        // 🔍 Get Floating Ammo Text
        GameObject ammoTextObj = GameObject.FindGameObjectWithTag("ammo text");
        if (ammoTextObj != null)
            ammoFloatingText = ammoTextObj.GetComponent<FloatingText>();
        else
            Debug.LogWarning("No GameObject with tag 'ammo text' found!");

        UpdateAmmoUI();

        if (ammoFloatingText != null)
            ammoFloatingText.gameObject.SetActive(false);

        // 🔊 Get Sound Source
        GameObject soundObj = GameObject.FindGameObjectWithTag("Sound");
        if (soundObj != null)
            soundSource = soundObj.GetComponent<AudioSource>();
        else
            Debug.LogWarning("No GameObject with tag 'Sound' found!");
    }
    void Update()
    {
        if (permanentlyDisabled) return;
        if (!canShoot) return;

        // 🔫 Manual shoot (UNCHANGED)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            TryShoot();
        }

       
        detectTimer += Time.deltaTime;

        if (detectTimer >= detectInterval)
        {
            detectTimer = 0f;
            DetectTargets(); // 👈 renamed
        }

        HandleTargeting(); // 👈 always runs smooth

        DrawSphereCast(firePoint.position, firePoint.forward, detectRadius, shootRange);
    }
  
    void TryShoot()
    {
        if (Time.time < lastShootTime + fireRate) return;

        if (currentAmmo <= 0)
        {
            ShowOutOfAmmo();
            return;
        }

        lastShootTime = Time.time;

        SpawnBullet();

        // 🔊 PLAY SOUND HERE
        PlayShootSound();

        currentAmmo--;
        UpdateAmmoUI();
    }
    void PlayShootSound()
    {
        if (soundSource != null && shootClip != null)
        {
            soundSource.PlayOneShot(shootClip);
            soundSource.pitch = Random.Range(0.9f, 1.1f);
        }
    }
    void DetectTargets()
    {
        Vector3 dir = firePoint.forward;
        Ray ray = new Ray(firePoint.position, dir);

        targetList.Clear();

        RaycastHit[] hits = Physics.SphereCastAll(ray, detectRadius, shootRange, targetLayers);

        foreach (RaycastHit h in hits)
        {
            GameObject hitObj = h.collider.transform.root.gameObject;

            if (hitObj.transform.root == transform.root)
                continue;

            if (hitObj.CompareTag("Zombie") ||
                hitObj.CompareTag("Obstacle") ||
                hitObj.CompareTag("Can"))
            {
                Vector3 dirToTarget = (hitObj.transform.position - firePoint.position).normalized;

                float dot = Vector3.Dot(dir, dirToTarget);
                if (dot < 0.3f) continue;

                if (!targetList.Contains(hitObj.transform))
                {
                    targetList.Add(hitObj.transform);
                }
            }
        }

        // 🔥 SORT ONCE
        targetList.Sort((a, b) =>
            Vector3.Distance(firePoint.position, a.position)
            .CompareTo(Vector3.Distance(firePoint.position, b.position)));
    }

    void HandleTargeting()
    {
        // 🔥 Remove dead targets
        targetList.RemoveAll(t => t == null);

        // ✅ KEEP LOCK IF STILL VALID
        if (lockedTarget != null && targetList.Contains(lockedTarget))
        {
            currentTarget = lockedTarget;
            return;
        }

        // 🔄 PICK NEW TARGET
        if (targetList.Count > 0)
        {
            lockedTarget = targetList[0]; // closest (already sorted)
            currentTarget = lockedTarget;
        }
        else
        {
            lockedTarget = null;
            currentTarget = null;
        }
    }
    void SpawnBullet()
    {
        if (!bulletPrefab || !firePoint) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (currentTarget != null)
        {
            Bullet bulletScript = bullet.GetComponent<Bullet>();

            if (bulletScript != null)
            {
                bulletScript.SetTarget(currentTarget);
                bulletScript.SetShooter(this); // 👈 IMPORTANT
            }
        }
    }
    void UpdateAmmoUI()
    {
        if (ammoCounterText != null)
        {
            ammoCounterText.text = $"{currentAmmo}/{maxAmmo}";
        }
    }

    void ShowOutOfAmmo()
    {
        if (isShowingAmmoText) return;

        if (ammoFloatingText != null)
        {
            ammoFloatingText.gameObject.SetActive(true);
            ammoFloatingText.SetText("OUT OF AMMO", Color.red);
        }

        StartCoroutine(AmmoTextCooldown());
    }

    IEnumerator AmmoTextCooldown()
    {
        isShowingAmmoText = true;
        yield return new WaitForSeconds(1f);
        isShowingAmmoText = false;
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    public void ForceStop()
    {
        permanentlyDisabled = true;
        canShoot = false;
    }
    void DrawSphereCast(Vector3 origin, Vector3 direction, float radius, float distance)
    {
        // 🔴 center line
        Debug.DrawRay(origin, direction * distance, Color.red);

        Vector3 end = origin + direction * distance;

        // 🔥 FIX: get correct perpendicular axes
        Vector3 up = firePoint.up;
        Vector3 right = firePoint.right;

        int segments = 24;
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector3 p1 = end + (right * Mathf.Cos(angle1) + up * Mathf.Sin(angle1)) * radius;
            Vector3 p2 = end + (right * Mathf.Cos(angle2) + up * Mathf.Sin(angle2)) * radius;

            Debug.DrawLine(p1, p2, Color.green);
        }
    }
    public void RemoveTarget(Transform t)
    {
        if (targetList.Contains(t))
        {
            targetList.Remove(t);
        }

        if (lockedTarget == t)
        {
            lockedTarget = null;
        }
    }
}