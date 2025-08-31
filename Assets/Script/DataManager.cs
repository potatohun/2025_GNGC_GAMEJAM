using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public const string HIGH_SCORE_KEY = "HighScore";
    public const string PLAY_TIME_KEY = "PlayTime";
    public const string PLAY_COUNT_KEY = "PlayCount";
    public const string GET_ITEM_HEART_KEY = "GetItemHeart";
    public const string GET_ITEM_ICE_KEY = "GetItemIce";
    public const string GET_ITEM_SHIELD_KEY = "GetItemShield";
    public const string GET_ITEM_ROCKET_KEY = "GetItemRocket";
    
    public static DataManager Instance;

    private float _currentPlayTime = 0;
    
    private bool _isPause = true;

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
    }

    public void Update() {
        if(_isPause == false) {
            _currentPlayTime += Time.deltaTime;
        }
    }

    public void SetPause(bool isPause)
    {
        _isPause = isPause;
    }

    public void SetHighScore(float score)
    {
        PlayerPrefs.SetFloat(HIGH_SCORE_KEY, score);
    }

    public float GetHighScore()
    {
        return PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0);
    }

    public void AddPlayTime()
    {
        PlayerPrefs.SetFloat(PLAY_TIME_KEY, GetPlayTime() + _currentPlayTime);
        _currentPlayTime = 0;
    }

    public float GetPlayTime()
    {
        return PlayerPrefs.GetFloat(PLAY_TIME_KEY, 0);
    }

    public void AddPlayCount()
    {
        PlayerPrefs.SetInt(PLAY_COUNT_KEY, GetPlayCount() + 1);
    }
    
    public int GetPlayCount()
    {
        return PlayerPrefs.GetInt(PLAY_COUNT_KEY, 0);
    }

    public void SetGetItem(string key, int isGet)
    {
        PlayerPrefs.SetInt(key, isGet);
    }

    public int GetGetItem(string key)
    {
        return PlayerPrefs.GetInt(key, 0);
    }

    public void DataReset()
    {
        PlayerPrefs.DeleteAll();
    }
}
