using UnityEngine;

public class Landmine : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 50;

    [Header("Visuals")]
    public MeshRenderer meshRenderer;

    // 🔥 PREFAB (NOT scene object)
    public ParticleSystem explodeEffectPrefab;

    bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            Debug.Log("Triggered by: " + other.name);

            // 💥 DAMAGE PLAYER
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player != null)
                player.TakeDamage(damage, "Landmine");

            // 👻 Hide mesh
            if (meshRenderer != null)
                meshRenderer.enabled = false;

            // 💥 SPAWN EXPLOSION (SAFE)
            if (explodeEffectPrefab != null)
            {
                ParticleSystem effect = Instantiate(
                    explodeEffectPrefab,
                    transform.position,
                    Quaternion.identity
                );

                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }

            // ❌ Disable collider
            GetComponent<Collider>().enabled = false;

            // 🧹 Destroy landmine
            Destroy(gameObject, 0.1f);
        }
    }
}