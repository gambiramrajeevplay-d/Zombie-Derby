using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerPause : MonoBehaviour

{

    private static bool _isMuted = false;

    public static bool IsMuted
    {
        get { return _isMuted; }
        set
        {
            _isMuted = value;
            AudioListener.pause = _isMuted;
            PlayerPrefs.SetInt("AudioMuted", _isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static void Initialize()
    {
        _isMuted = PlayerPrefs.GetInt("AudioMuted", 0) == 1;
        AudioListener.pause = _isMuted;
    }
}


