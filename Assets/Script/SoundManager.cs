using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SoundData {
    public string name;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [SerializeField] private AudioMixer _audioMixer;

    [SerializeField] private List<SoundData> _soundList;

    private AudioSource _audioSource;

    void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }

        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(string soundName) {
        SoundData soundData = _soundList.Find(x => x.name == soundName);
        if(soundData != null) {
            _audioSource.PlayOneShot(soundData.clip);
        }
    }

    public void SetBGMVolume(float volume) {
        // 0 ~ 1 값을 -80 ~ 20 사이의 값으로 변환
        float db = Mathf.Lerp(-80, 20, volume);
        _audioMixer.SetFloat("BGM", db);
    }

    public void SetSFXVolume(float volume) {
        float db = Mathf.Lerp(-80, 20, volume);
        _audioMixer.SetFloat("SFX", db);
    }
}
