using UnityEngine;
using UnityEngine.SceneManagement;

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
    public GameObject levelRoot;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ResetInputTimer();
        Time.timeScale = 1f;
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
    public void FailLevel()
    {
        if (levelEnded) return;

        levelEnded = true;

        // Show fail UI
        if (failPanel != null)
            failPanel.SetActive(true);

        // Disable player control (optional)
        Time.timeScale = 0f;
    }

    // 🔄 RESTART FUNCTION
    public void RestartLevel()
    {
        Time.timeScale = 1f; // 🔥 IMPORTANT

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void Home()
    {
        Time.timeScale = 1f; // 🔥 reset time
        SceneManager.LoadScene("UI_Dummy");
    }
}