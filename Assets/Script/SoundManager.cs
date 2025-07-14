using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundData {
    public string name;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

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
}
