using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{

    public int levelToLoad;

    public void LoadLevel()
    {

        PlayerPrefs.SetInt(StringsData.levelToLoad,levelToLoad);
        SceneManager.LoadScene(1);
    }

   
}
