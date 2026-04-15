using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public int obstacleDamage = 15;
    public int zombieDamage = 3;
    public int canDamage = 15;

    private FloatingText floatingText;        // default damage
    private FloatingText zombieFloatingText;  // 🧟 zombie damage

    private Rigidbody rb;

    private Image healthFill;
    private TextMeshProUGUI healthText;

    private TextMeshProUGUI healText;

    [Header("Damage Cooldown")]
    public float damageCooldown = 1f;
    private float lastDamageTime;

    private Queue<(string, Color)> textQueue = new Queue<(string, Color)>();
    private bool isShowing = false;
    private FloatingText canFloatingText;
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();

        // 🔍 Default Floating Text
        GameObject canvasObj = GameObject.FindGameObjectWithTag("In_Game");
        if (canvasObj != null)
        {
            floatingText = canvasObj.GetComponentInChildren<FloatingText>(true);
        }

        // 🔍 Can Floating Text
        GameObject canObj = GameObject.FindGameObjectWithTag("CanHealth");
        if (canObj != null)
        {
            canFloatingText = canObj.GetComponent<FloatingText>();
        }

        // 🔍 Zombie Floating Text
        GameObject zombieObj = GameObject.FindGameObjectWithTag("ZombieHeath");
        if (zombieObj != null)
        {
            zombieFloatingText = zombieObj.GetComponent<FloatingText>();
        }

        // 🔍 Heal Text
        GameObject healObj = GameObject.FindGameObjectWithTag("Health+50");
        if (healObj != null)
            healText = healObj.GetComponent<TextMeshProUGUI>();

        // 🔍 Health Fill
        GameObject fillObj = GameObject.FindGameObjectWithTag("Health_Fill");
        if (fillObj != null)
            healthFill = fillObj.GetComponent<Image>();

        // 🔍 Health Text
        GameObject textObj = GameObject.FindGameObjectWithTag("Health_Text");
        if (textObj != null)
            healthText = textObj.GetComponent<TextMeshProUGUI>();

        UpdateHealthUI();
    }

    private void OnTriggerEnter(Collider other)
    {
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

    // 🔻 DAMAGE
    // 🔻 DAMAGE
    public void TakeDamage(int damage, string type)
    {
        lastDamageTime = Time.time;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 🧟 Zombie text
        if (type == "Zombie" && zombieFloatingText != null)
        {
            zombieFloatingText.StopAllCoroutines();
            zombieFloatingText.gameObject.SetActive(false);
            zombieFloatingText.gameObject.SetActive(true);

            zombieFloatingText.SetText("-" + damage, Color.red);
        }
        // 🛢️ Can text (NEW)
        else if (type == "Can" && canFloatingText != null)
        {
            canFloatingText.StopAllCoroutines();
            canFloatingText.gameObject.SetActive(false);
            canFloatingText.gameObject.SetActive(true);

            canFloatingText.SetText("-" + damage, Color.red);
        }
        else
        {
            EnqueueDamageText("-" + damage, GetColor(type));
        }

        UpdateHealthUI();

        if (currentHealth <= 0)
            Die();
    }
    // 🔺 HEAL
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        ShowHealText(amount);
        UpdateHealthUI();
    }

    // 🔥 QUEUE DAMAGE TEXT
    void EnqueueDamageText(string text, Color color)
    {
        textQueue.Enqueue((text, color));

        if (!isShowing)
            StartCoroutine(ProcessQueue());
    }

    IEnumerator ProcessQueue()
    {
        isShowing = true;

        while (textQueue.Count > 0)
        {
            var item = textQueue.Dequeue();

            if (floatingText != null)
            {
                floatingText.gameObject.SetActive(true);
                floatingText.SetText(item.Item1, item.Item2);
            }

            yield return new WaitForSeconds(0.4f);
        }

        isShowing = false;
    }

    // 🎨 COLORS
    Color GetColor(string type)
    {
        if (type == "Can") return Color.red;
        if (type == "Obstacle") return Color.yellow;
        if (type == "Landmine") return Color.red;

        return Color.white;
    }

    // ✅ HEAL TEXT
    void ShowHealText(int amount)
    {
        if (healText == null) return;

        healText.gameObject.SetActive(true);
        healText.text = "+" + amount;
        healText.color = Color.green;

        StopCoroutine("HideHealText");
        StartCoroutine("HideHealText");
    }

    IEnumerator HideHealText()
    {
        yield return new WaitForSeconds(1f);
        healText.gameObject.SetActive(false);
    }

    // ❤️ UI
    void UpdateHealthUI()
    {
        float healthPercent = (float)currentHealth / maxHealth;

        if (healthFill != null)
            healthFill.fillAmount = healthPercent;

        if (healthText != null)
            healthText.text = currentHealth.ToString();
    }

    void Die()
    {
        Debug.Log("Player Died!");
        GameManager.Instance.PlayerDied();
    }
}