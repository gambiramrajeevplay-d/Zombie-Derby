using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Fail Settings")]
    public float noInputTimeLimit = 5f;

    private float lastInputTime;
    private bool levelEnded = false;

    [Header("UI")]
    public GameObject failPanel;
    public GameObject passPanel;

    [Header("Level")]
    public GameObject levelRoot; // 👈 assign your full level parent

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ResetInputTimer();
        Time.timeScale = 1f; // reset if restarted
    }

    void Update()
    {
        if (levelEnded) return;

        CheckPlayerInput();
        CheckIdleFail();
    }

    void CheckPlayerInput()
    {
        if (Input.anyKey || Input.GetAxis("Vertical") != 0)
        {
            ResetInputTimer();
        }
    }

    void CheckIdleFail()
    {
        if (Time.time - lastInputTime > noInputTimeLimit)
        {
            LevelFail("No Input!");
        }
    }

    public void ResetInputTimer()
    {
        lastInputTime = Time.time;
    }

    public void PlayerDied()
    {
        if (levelEnded) return;

        LevelFail("Player Dead!");
    }

    public void LevelPass()
    {
        if (levelEnded) return;

        levelEnded = true;

        Time.timeScale = 0f;

        if (levelRoot != null)
            levelRoot.SetActive(false);

        if (passPanel != null)
            passPanel.SetActive(true);
    }

    public void LevelFail(string reason)
    {
        if (levelEnded) return;

        levelEnded = true;

        Time.timeScale = 0f;

        if (levelRoot != null)
            levelRoot.SetActive(false);

        if (failPanel != null)
            failPanel.SetActive(true);

        Debug.Log("LEVEL FAILED: " + reason);
    }
}