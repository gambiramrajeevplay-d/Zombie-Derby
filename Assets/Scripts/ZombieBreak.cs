using UnityEngine;

public class ZombieBreak : MonoBehaviour
{
    [Header("References")]
    public GameObject animatedBody;
    public GameObject ragdollRoot;

    [Header("Ragdoll Parts")]
    public Rigidbody[] ragdollBodies;
    public Collider[] ragdollColliders;

    public Animator animator;

    [Header("Break Settings (HEAVY FEEL)")]
    public float explosionForce = 0f;   // ❌ NO explosion
    public float upwardForce = 0f;
    public float radius = 1f;
    public float directionalForce = 8f; // VERY LOW push
    public float spinForce = 2f;        // VERY LOW spin

    [Header("Cleanup")]
    public float destroyDelay = 3f;

    private bool broken = false;

    void Start()
    {
        ragdollRoot.SetActive(true);

        foreach (Rigidbody rb in ragdollBodies)
            if (rb != null) rb.isKinematic = true;

        foreach (Collider col in ragdollColliders)
            if (col != null) col.enabled = false;

        foreach (Renderer r in ragdollRoot.GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }

    public void Break(Vector3 hitPoint, Vector3 hitDirection)
    {
        if (broken) return;
        broken = true;

        if (animatedBody != null)
            animatedBody.SetActive(false);

        if (animator != null)
            animator.enabled = false;

        // Show ragdoll
        foreach (Renderer r in ragdollRoot.GetComponentsInChildren<Renderer>())
            r.enabled = true;

        // Break joints
        CharacterJoint[] joints = ragdollRoot.GetComponentsInChildren<CharacterJoint>();
        foreach (CharacterJoint joint in joints)
            Destroy(joint);

        // Enable colliders
        foreach (Collider col in ragdollColliders)
            if (col != null) col.enabled = true;

        hitDirection.Normalize();

        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb == null) continue;

            rb.isKinematic = false;
            rb.transform.parent = null;

            // ❌ NO explosion

            // ✅ VERY LOW forward push
            rb.AddForce(hitDirection * directionalForce, ForceMode.Impulse);

            // ✅ VERY SMALL side motion
            Vector3 side = Vector3.Cross(hitDirection, Vector3.up);
            rb.AddForce(side * spinForce, ForceMode.Impulse);

            // ✅ VERY LOW rotation
            rb.AddTorque(Random.onUnitSphere * 2f, ForceMode.Impulse);

            Destroy(rb.gameObject, destroyDelay);
        }
    }
}