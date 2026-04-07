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

    [Header("Shoot Sound")]
    public AudioClip shootClip;

    private AudioSource soundSource;

    [Header("Cone Detection")]
    public float detectRadius = 1.5f;
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

        // 🔥 SphereCast instead of Raycast
        bool hasHit = Physics.SphereCast(ray, detectRadius, out hit, shootRange, targetLayers);

        // Debug cone (visual help)
        DrawSphereCast(firePoint.position, Vector3.left, detectRadius, shootRange);
        //if (hasHit)
        //{
        //    GameObject hitObj = hit.collider.gameObject;

        //    if (hitObj.CompareTag("Zombie") ||
        //        hitObj.CompareTag("Obstacle") ||
        //        hitObj.CompareTag("Can"))
        //    {
        //        // 🎯 AUTO AIM
        //        Vector3 direction = (hit.point - firePoint.position).normalized;
        //        Quaternion lookRotation = Quaternion.LookRotation(direction);
        //        firePoint.rotation = lookRotation;
        //    }
        //}
        Collider[] hits = Physics.OverlapSphere(firePoint.position, shootRange, targetLayers);

        foreach (Collider col in hits)
        {
            Vector3 dirToTarget = (col.transform.position - firePoint.position).normalized;

            float angle = Vector3.Angle(Vector3.left, dirToTarget);

            if (angle < 30f) // 🔥 cone angle
            {
                if (col.CompareTag("Zombie") ||
                    col.CompareTag("Obstacle") ||
                    col.CompareTag("Can"))
                {
                    Quaternion lookRotation = Quaternion.LookRotation(dirToTarget);
                    firePoint.rotation = lookRotation;
                    break;
                }
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
    void DrawSphereCast(Vector3 origin, Vector3 direction, float radius, float distance)
    {
        // Draw center line
        Debug.DrawRay(origin, direction * distance, Color.red);

        // Draw multiple offset rays (fake cone visual)
        Debug.DrawRay(origin + Vector3.up * radius, direction * distance, Color.green);
        Debug.DrawRay(origin - Vector3.up * radius, direction * distance, Color.green);
        Debug.DrawRay(origin + Vector3.right * radius, direction * distance, Color.green);
        Debug.DrawRay(origin - Vector3.right * radius, direction * distance, Color.green);
    }
}