using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public int obstacleDamage = 15;
    public int zombieDamage = 3;
    public int canDamage = 15;

    private FloatingText floatingText;
    private Rigidbody rb;

    private Image healthFill;
    private TextMeshProUGUI healthText;

    [Header("Damage Cooldown")]
    public float damageCooldown = 1f;
    private float lastDamageTime;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();

        // 🔍 Floating Text
        GameObject canvasObj = GameObject.FindGameObjectWithTag("In_Game");
        if (canvasObj != null)
        {
            floatingText = canvasObj.GetComponentInChildren<FloatingText>(true);
        }

        // 🔍 Health Fill Image
        GameObject fillObj = GameObject.FindGameObjectWithTag("Health_Fill");
        if (fillObj != null)
            healthFill = fillObj.GetComponent<Image>();

        // 🔍 Health Text
        // 🔍 Health Text (TextMeshPro)
        GameObject textObj = GameObject.FindGameObjectWithTag("Health_Text");
        if (textObj != null)
            healthText = textObj.GetComponent<TextMeshProUGUI>();

        UpdateHealthUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        // ❌ Prevent multiple hits spam
        if (Time.time < lastDamageTime + damageCooldown) return;

        if (other.CompareTag("Obstacle"))
        {
            TakeDamage(obstacleDamage, "Obstacle");
        }
        else if (other.CompareTag("Zombie"))
        {
            TakeDamage(zombieDamage, "Zombie");
        }
        else if (other.CompareTag("Can"))
        {
            TakeDamage(canDamage, "Can");
        }
    }

    void TakeDamage(int damage, string type)
    {
        lastDamageTime = Time.time;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        ShowFloatingText(damage, type);
        UpdateHealthUI();

        if (currentHealth <= 0)
            Die();
    }

    void ShowFloatingText(int damage, string type)
    {
        if (floatingText == null) return;

        floatingText.gameObject.SetActive(true);

        // 🎨 COLOR LOGIC
        if (type == "Can")
            floatingText.SetText("-" + damage, Color.red);        // 🔴 CAN
        else if (type == "Obstacle")
            floatingText.SetText("-" + damage, Color.yellow);     // 🟡 BOX
        else
            floatingText.SetText("-" + damage, Color.white);     // 🟡 ZOMBIE
    }

    void UpdateHealthUI()
    {
        float healthPercent = (float)currentHealth / maxHealth;

        // ❤️ Fill Image
        if (healthFill != null)
        {
            healthFill.fillAmount = healthPercent;
        }

        // 📝 Text
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString();
        }
    }

    void Die()
    {
        Debug.Log("Player Died!");
    }
}