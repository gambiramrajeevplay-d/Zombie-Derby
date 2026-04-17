using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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

    [Header("UI Sounds")]
    public AudioClip failClip;
    public AudioClip passClip;
    private AudioSource uiAudio;

    [Header("Currency")]
    public int levelReward = 100;
    public TextMeshProUGUI coinText; // 💰 assign in inspector
    private AudioSource musicAudio;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ResetInputTimer();
        Time.timeScale = 1f;

        // 🔊 Get UI Audio
        GameObject soundObj = GameObject.FindGameObjectWithTag("UISound");
        if (soundObj != null)
        {
            uiAudio = soundObj.GetComponent<AudioSource>();
        }
        // 🎵 Get Music Audio
        GameObject musicObj = GameObject.FindGameObjectWithTag("Music");
        if (musicObj != null)
        {
            musicAudio = musicObj.GetComponent<AudioSource>();
        }
        // 💰 Initialize coin UI

        UpdateCoinUI();
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

    // ✅ LEVEL PASS
    public void LevelPass()
    {
        // 🔇 Stop music
        if (musicAudio != null)
        {
            musicAudio.Stop();
        }

        if (levelEnded) return;

        levelEnded = true;
        Time.timeScale = 0f;

        if (levelRoot != null)
            levelRoot.SetActive(false);

        if (passPanel != null)
            passPanel.SetActive(true);

        // 🔊 Sound
        if (uiAudio != null && passClip != null)
            uiAudio.PlayOneShot(passClip);

        // 💰 GIVE COINS
        if (CurrecnyManager.instance != null)
        {
            CurrecnyManager.instance.AddCurrency(levelReward);

            // 🔥 Show reward ONLY
            if (coinText != null)
            {
                coinText.text = "+" + levelReward.ToString();
            }
        }

        // 🔓 UNLOCK NEXT LEVEL
        int currentLevel = PlayerPrefs.GetInt(StringsData.playerLevel, 1);
        int buildIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentLevel <= buildIndex)
        {
            PlayerPrefs.SetInt(StringsData.playerLevel, currentLevel + 1);
            PlayerPrefs.Save();
        }
    }

    // ❌ LEVEL FAIL
    public void LevelFail(string reason)
    {
        if (musicAudio != null)
        {
            musicAudio.Stop();
        }

        if (levelEnded) return;

        levelEnded = true;
        Time.timeScale = 0f;

        if (levelRoot != null)
            levelRoot.SetActive(false);

        if (failPanel != null)
            failPanel.SetActive(true);

        // 🔊 Sound
        if (uiAudio != null && failClip != null)
            uiAudio.PlayOneShot(failClip);

        Debug.Log("LEVEL FAILED: " + reason);
    }

    public void FailLevel()
    {
        if (levelEnded) return;

        levelEnded = true;

        if (failPanel != null)
            failPanel.SetActive(true);

        if (uiAudio != null && failClip != null)
            uiAudio.PlayOneShot(failClip);

        Time.timeScale = 0f;
    }

    // 💰 UPDATE COIN UI
    void UpdateCoinUI()
    {
        if (coinText != null && CurrecnyManager.instance != null)
        {
            coinText.text = CurrecnyManager.instance.GetCurrency().ToString();
        }
    }

    // 🔄 RESTART
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 🏠 HOME
    public void Home()
    {
        Time.timeScale = 1f;

        // 🔥 Tell MainMenu to open subscription panel
        PlayerPrefs.SetInt("ShowSubscriptionPanel", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("UI"); // or your main menu scene
    }
}