using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    [Header("Game Settings")]
    [SerializeField] private int _currentHeart = 5;
    [SerializeField] private float _maxHeight = 0;

    [Header("UI")]
    [SerializeField] private List<Image> _heartList;
    [SerializeField] private Transform _player;
    [SerializeField] private TextMeshProUGUI _maxHeightText;

    public GameObject _gameOverUI;
    public TMP_Text _finalHeightText;
    public GameObject _bgmObject;
    public GameObject _gameOverBgmObject;
    
    void Start() {
        Time.timeScale = 1;
        _gameOverUI.SetActive(false);
        _gameOverUI.GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySound("ButtonClick");
            // 현재 씬을 다시 로드
            SceneManager.LoadScene("Intro");
        });
    }

    void Awake() {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        _currentHeart = _heartList.Count;
    }

    public void SetMaxHeight(float maxHeight) {
        if(maxHeight > _maxHeight) {
            _maxHeight = maxHeight;
            SetPlayerPosition();
            _maxHeightText.text = _maxHeight.ToString("F0") + "m";
        }
    }

    private void SetPlayerPosition() {
        _player.DOMoveY(_maxHeight, 2f).SetEase(Ease.InOutSine);
    }

    public void Damage() {
        // 이미 게임 오버 상태라면 데미지를 받지 않음
        if (_currentHeart <= 0) {
            return;
        }

        _currentHeart--;

        // 하트를 점차 어둡게 만드는 DOTween 애니메이션
        if (_currentHeart >= 0 && _currentHeart < _heartList.Count) {
            _heartList[_currentHeart].DOColor(new Color(0.25f, 0.25f, 0.25f), 0.5f).OnComplete(() => {
                if (_currentHeart == 0) {
                    Debug.Log("GameOver");
                    //Time.timeScale = 0;
                    _gameOverUI.SetActive(true);
                    _bgmObject.SetActive(false);
                    _gameOverBgmObject.SetActive(true);
                    _finalHeightText.text = _maxHeight.ToString("F0") + "m";
                    BlockManager.Instance.EndGame();
                }
            });
        }
    }    
}
