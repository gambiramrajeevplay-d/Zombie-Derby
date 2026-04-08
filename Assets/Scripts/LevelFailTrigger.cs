using UnityEngine;

public class LevelFailTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.LevelFail("Hit Fail Zone");
        }
    }
}