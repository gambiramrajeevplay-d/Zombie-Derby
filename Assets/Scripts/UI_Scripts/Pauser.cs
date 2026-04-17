using Script;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pauser : MonoBehaviour
{
    public static Pauser instance;

    [Header("UI")]
    public GameObject PausePannel;
    public GameObject LevelObject;
    public GameObject PauseButton;

    [Header("Sound UI")]
    public Image soundIcon;
    public Sprite sound_on;
    public Sprite sound_off;

    // 🔒 GLOBAL PAUSE LOCK
    public static bool PauseLocked = false;

    private void Awake()
    {
        instance = this;
        AudioManagerPause.Initialize();
    }

    private void OnEnable()
    {
        PauseButton.SetActive(!AndroidTV.IsAndroidOrFireTv());
        UpdateSoundIcon(); // 🔥 update icon when opened
    }

    private void Start()
    {
        PauseButton.SetActive(!AndroidTV.IsAndroidOrFireTv());
        UpdateSoundIcon(); // 🔥 initial icon
    }

    void Update()
    {
        if (PauseLocked) return;

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.passPanel.activeSelf ||
                GameManager.Instance.failPanel.activeSelf)
                return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (PauseLocked) return;

        LevelObject.SetActive(false);
        PausePannel.SetActive(true);

        if (!AndroidTV.IsAndroidOrFireTv())
            PauseButton.SetActive(false);

        Time.timeScale = 0f;

        UpdateSoundIcon(); // 🔥 refresh icon on open
    }

    public void Resume()
    {
        LevelObject.SetActive(true);
        PausePannel.SetActive(false);

        if (!AndroidTV.IsAndroidOrFireTv())
            PauseButton.SetActive(true);

        Time.timeScale = 1f;
    }

    public void MM()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("UI");
    }

    // 🔊 TOGGLE SOUND
    public void ToggleSound()
    {
        AudioManagerPause.IsMuted = !AudioManagerPause.IsMuted;
        UpdateSoundIcon();
    }

    // 🔊 UPDATE ICON (same as MainMenu)
    private void UpdateSoundIcon()
    {
        if (soundIcon != null)
        {
            soundIcon.sprite = AudioManagerPause.IsMuted ? sound_off : sound_on;
        }
    }

    public static void LockPause()
    {
        PauseLocked = true;

        if (instance != null)
        {
            instance.PausePannel.SetActive(false);
            instance.LevelObject.SetActive(true);
        }
    }

    public static void UnlockPause()
    {
        PauseLocked = false;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            Debug.Log("App lost focus");

            if (!PauseLocked &&
                GameManager.Instance != null &&
                !GameManager.Instance.passPanel.activeSelf &&
                !GameManager.Instance.failPanel.activeSelf)
            {
                Pause();
            }
        }
        else
        {
            Debug.Log("App gained focus");
            AudioListener.volume = 1f;
        }
    }
}