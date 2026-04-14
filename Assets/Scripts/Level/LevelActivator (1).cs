using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelActivator : MonoBehaviour
{
    public GameObject[] levels;

    private void Awake()
    {
        int levelToLoad = PlayerPrefs.GetInt(StringsData.levelToLoad);
        levels[levelToLoad].SetActive(true);
        
    }

}
