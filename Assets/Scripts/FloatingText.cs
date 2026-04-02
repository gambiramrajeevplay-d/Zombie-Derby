using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 50f;
    public float fadeSpeed = 2f;
    public float lifetime = 1f;

    private TextMeshProUGUI text;
    private float timer;
    private float delayTimer = 0.05f; // small delay before movement

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();

        // Reset timers
        timer = lifetime;
        delayTimer = 0.05f;

        // Reset alpha
        Color c = text.color;
        c.a = 1f;
        text.color = c;
    }

    void Update()
    {
        // Small delay before starting movement
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        // ✅ Move DOWN instead of up
        transform.localPosition += Vector3.down * moveSpeed * Time.deltaTime;

        // Fade out
        Color c = text.color;
        c.a -= fadeSpeed * Time.deltaTime;
        text.color = c;

        // Lifetime handling
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetText(string message, Color color)
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();

        text.text = message;

        color.a = 1f;
        text.color = color;
    }
}