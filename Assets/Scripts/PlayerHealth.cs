using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public int obstacleDamage = 15;
    public int zombieDamage = 3;

    private FloatingText floatingText;
    private Rigidbody rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();

        GameObject canvasObj = GameObject.FindGameObjectWithTag("In_Game");
        if (canvasObj != null)
        {
            floatingText = canvasObj.GetComponentInChildren<FloatingText>(true);
        }
    }

    // ⚡ INSTANT TRIGGER DETECTION
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            TakeDamage(obstacleDamage);
        }
        else if (other.CompareTag("Zombie"))
        {
            TakeDamage(zombieDamage);

           
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        ShowFloatingText(damage);

        if (currentHealth <= 0)
            Die();
    }

    void ShowFloatingText(int damage)
    {
        if (floatingText == null) return;

        floatingText.gameObject.SetActive(true);

        if (damage == obstacleDamage)
            floatingText.SetText("-" + damage, Color.red);
        else
            floatingText.SetText("-" + damage, Color.yellow);
    }

    void Die()
    {
        Debug.Log("Player Died!");
    }
}