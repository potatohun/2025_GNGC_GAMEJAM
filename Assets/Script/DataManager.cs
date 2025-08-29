using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void SetHighScore(float score)
    {
        PlayerPrefs.SetFloat("HighScore", score);
    }

    public float GetHighScore()
    {
        return PlayerPrefs.GetFloat("HighScore", -1);
    }

    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
    }
}
