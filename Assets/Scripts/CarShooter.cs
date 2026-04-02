using UnityEngine;
using System.Collections;
using TMPro;

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

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        if (ammoFloatingText != null)
            ammoFloatingText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (permanentlyDisabled) return;
        if (!canShoot) return;

        // 🔫 Manual shoot (UNCHANGED)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryShoot();
        }

        // 🔥 Auto shoot
        DetectAndShoot();
    }

    void DetectAndShoot()
    {
        Ray ray = new Ray(firePoint.position, Vector3.left);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * shootRange, Color.red);

        if (Physics.Raycast(ray, out hit, shootRange, targetLayers))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj.CompareTag("Zombie") ||
                hitObj.CompareTag("Obstacle") ||
                hitObj.CompareTag("Can"))
            {
                // 🎯 AUTO AIM ONLY (NO SHOOT)
                Vector3 direction = (hit.point - firePoint.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                firePoint.rotation = lookRotation;
            }
        }
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
        currentAmmo--;

        UpdateAmmoUI();
    }

    void SpawnBullet()
    {
        if (!bulletPrefab || !firePoint)
        {
            Debug.LogWarning("Missing bulletPrefab or firePoint");
            return;
        }

        // 🔥 NO CHANGE to bullet direction
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
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
}