using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private RectTransform _fullCanvas;
    [SerializeField] private PlayableDirector _introDirector;
    [SerializeField] private float _durationIntro = 9f;

    [SerializeField] private Ease _easeMode = Ease.OutBack;
    [SerializeField] private float _easeDuration = 1f;

    [Header("Panels")]
    [SerializeField] private RectTransform _mainPanel;
    [SerializeField] private RectTransform _leftPanel;
    [SerializeField] private RectTransform _rightPanel;
    
    [Header("Black Panel")]
    [SerializeField] private RectTransform _blackPanel_up;
    [SerializeField] private RectTransform _blackPanel_down;
    [SerializeField] private GameObject _skipHolder;
    [SerializeField] private Image _skipIcon;

    [Header("Setting Panel")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private TMP_Dropdown _windowModeDropdown;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    private float _screenWidth;
    private float _screenHeight;

    private List<Resolution> _uniqueResolutions;
    private int _currentResolutionIndex = 0;
    private int _currentWindowModeIndex = 0;

    private float _bgmVolume = 0.8f;
    private float _sfxVolume = 0.8f;

    // PlayerPrefs 키 상수
    private const string RESOLUTION_INDEX_KEY = "ResolutionIndex";
    private const string WINDOW_MODE_KEY = "WindowMode";

    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    // 인트로 스킵 관련 변수
    private bool _isIntroPlaying = false;
    private float _keyHoldTime = 0f;
    private const float _requiredHoldTime = 1f; // 1초간 누르기
    private bool _hasSkipped = false;

    void Awake(){
        // Resolution Dropdown Setting
        _uniqueResolutions = GetSupportedResolutions();

        // 저장된 설정 불러오기 (해상도 목록 생성 후)
        LoadSettings();

        // Resolution Dropdown Setting
        _resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < _uniqueResolutions.Count; i++) {
            string option = _uniqueResolutions[i].width + "x" + _uniqueResolutions[i].height;
            options.Add(option);
        }
        _resolutionDropdown.AddOptions(options);
        
        // Window Mode Dropdown Setting
        _windowModeDropdown.ClearOptions();
        List<string> windowModeOptions = new List<string> { "Fullscreen", "Windowed" };
        _windowModeDropdown.AddOptions(windowModeOptions);
    }

    void Start() {
        // Apply Panel Setting
        _resolutionDropdown.value = _currentResolutionIndex;
        _windowModeDropdown.value = _currentWindowModeIndex;

        Resolution selectedRes = _uniqueResolutions[_currentResolutionIndex];
        bool isFullscreen = _currentWindowModeIndex == 0;
        
        // Apply Settings
        Screen.SetResolution(selectedRes.width, selectedRes.height, isFullscreen);
        
        SettingPanel();
        SetBlackPanel();

        // BGM Slider Settings
        _bgmSlider.value = _bgmVolume;
        _sfxSlider.value = _sfxVolume;
        SoundManager.Instance.SetBGMVolume(_bgmVolume);
        SoundManager.Instance.SetSFXVolume(_sfxVolume);

        // Resolution Dropdown Event
        _resolutionDropdown.onValueChanged.AddListener((index) => {
            _currentResolutionIndex = index;
            ApplySettings();
        });

        // Window Mode Dropdown Event
        _windowModeDropdown.onValueChanged.AddListener((index) => {
            _currentWindowModeIndex = index;
            ApplySettings();
        });

        // BGM Slider Event
        _bgmSlider.onValueChanged.AddListener((volume) => {
            SoundManager.Instance.SetBGMVolume(volume);
            _bgmVolume = volume;
            SaveSettings();
        });

        // SFX Slider Event
        _sfxSlider.onValueChanged.AddListener((volume) => {
            SoundManager.Instance.SetSFXVolume(volume);
            _sfxVolume = volume;
            SaveSettings();
        });
    }

    // 저장된 설정 불러오기
    private void LoadSettings() {
        // 저장된 해상도 인덱스 불러오기
        if (PlayerPrefs.HasKey(RESOLUTION_INDEX_KEY)) {
            int savedResolutionIndex = PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY);
            // 저장된 인덱스가 유효한 범위인지 확인
            if (savedResolutionIndex >= 0 && savedResolutionIndex < _uniqueResolutions.Count)
                _currentResolutionIndex = savedResolutionIndex;
            else
                _currentResolutionIndex = _uniqueResolutions.Count - 1;
        } else
            _currentResolutionIndex = _uniqueResolutions.Count - 1;

        // 저장된 윈도우 모드 불러오기
        if (PlayerPrefs.HasKey(WINDOW_MODE_KEY)) {
            _currentWindowModeIndex = PlayerPrefs.GetInt(WINDOW_MODE_KEY);
        } else {
            // 저장된 설정이 없으면 현재 화면 상태로 초기화
            _currentWindowModeIndex = Screen.fullScreen ? 0 : 1;
        }

        // 저장된 BGM 볼륨 불러오기
        if (PlayerPrefs.HasKey(BGM_VOLUME_KEY)) {
            _bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY);
        } else {
            _bgmVolume = 0.8f;
        }

        // 저장된 SFX 볼륨 불러오기
        if (PlayerPrefs.HasKey(SFX_VOLUME_KEY)) {
            _sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY);
        } else {
            _sfxVolume = 0.8f;
        }
    }

    // 지원하는 4가지 해상도만 반환
    private List<Resolution> GetSupportedResolutions() {
        List<Resolution> supportedResolutions = new List<Resolution>();
        
        // 지원하는 해상도들 (16:9 비율)
        Resolution[] targetResolutions = {
            new Resolution { width = 1920, height = 1080 },  // 1080p
            new Resolution { width = 2560, height = 1440 },  // 1440p
            new Resolution { width = 3840, height = 2160 }   // 2160p (4K)
        };

        Resolution[] maxResolution = Screen.resolutions;
        
        // 현재 모니터에서 지원하는 해상도만 추가
        for (int i = 0; i < targetResolutions.Length; i++) {
            Resolution res = targetResolutions[i];
            if (res.width <= maxResolution[maxResolution.Length - 1].width && res.height <= maxResolution[maxResolution.Length - 1].height) {
                supportedResolutions.Add(res);
            }
        }

        // 만약 지원하는 해상도가 없다면 현재 해상도만 추가
        if (supportedResolutions.Count == 0) {
            supportedResolutions.Add(Screen.currentResolution);
        }

        return supportedResolutions;
    }

    // 설정 저장하기
    private void SaveSettings() {
        PlayerPrefs.SetInt(RESOLUTION_INDEX_KEY, _currentResolutionIndex);
        PlayerPrefs.SetInt(WINDOW_MODE_KEY, _currentWindowModeIndex);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, _bgmVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, _sfxVolume);
        PlayerPrefs.Save();
    }

    // 설정을 즉시 적용하는 메서드
    private void ApplySettings() {
        StartCoroutine(ApplySettingsCoroutine());
    }

    // 설정 적용을 위한 코루틴
    private IEnumerator ApplySettingsCoroutine() {
        // 해상도와 윈도우 모드 동시 적용
        Resolution selectedRes = _uniqueResolutions[_currentResolutionIndex];
        bool isFullscreen = _currentWindowModeIndex == 0;
        
        // Apply Settings
        Screen.SetResolution(selectedRes.width, selectedRes.height, isFullscreen);
        
        // 설정 저장
        SaveSettings();
        
        // 한 프레임 대기하여 설정이 완료되도록 함
        yield return new WaitForEndOfFrame();
        
        // UI 업데이트
        SettingPanel();
        SetBlackPanel();

        // 화면 크기에 따른 위치 조정
        _fullCanvas.anchoredPosition = new Vector2(-_screenWidth, 0);
    }

    public void OnClickStartButton() {
        SoundManager.Instance.PlaySound("ButtonClick");
        FocusBlackPanel();
        _fullCanvas.DOAnchorPosY(_screenHeight, _easeDuration).SetEase(_easeMode).OnComplete(() => {
            _introDirector.Play();
            _isIntroPlaying = true;
            _hasSkipped = false;
            _keyHoldTime = 0f;
            // 스킵 아이콘 초기화
            if (_skipIcon != null) {
                _skipIcon.fillAmount = 0f;
                _skipHolder.SetActive(true);
            }
            Invoke("CloseBlackPanel", _durationIntro - 1f);
            Invoke("LoadGameScene", _durationIntro);
        });
    }

    public void LoadGameScene() {
        _introDirector.Stop();
        _isIntroPlaying = false;
        // 스킵 아이콘 숨기기
        if (_skipIcon != null) {
            _skipHolder.SetActive(false);
        }
        SceneManager.LoadScene("Game");
    }

    public void OnClickHelpButton() {
        SoundManager.Instance.PlaySound("ButtonClick");
        _fullCanvas.DOAnchorPosX(_screenWidth, _easeDuration).SetEase(_easeMode).OnComplete(() => {
        });

        // 키보드 입력 무시
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnClickBackButton() {
        SoundManager.Instance.PlaySound("ButtonClick");
        _fullCanvas.DOAnchorPosX(0, _easeDuration).SetEase(_easeMode);

        // 키보드 입력 무시
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnClickSettingButton() {
        SoundManager.Instance.PlaySound("ButtonClick");
        _fullCanvas.DOAnchorPosX(-_screenWidth, _easeDuration).SetEase(_easeMode).OnComplete(() => {
        });

        // 키보드 입력 무시
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnClickExitButton() {
        SoundManager.Instance.PlaySound("ButtonClick");
        Application.Quit();

        // 키보드 입력 무시
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void FocusBlackPanel() {
        _blackPanel_up.DOAnchorPosY(_screenHeight * 0.25f, _easeDuration).SetEase(_easeMode);
        _blackPanel_down.DOAnchorPosY(-_screenHeight * 0.25f, _easeDuration).SetEase(_easeMode);
    }

    public void CloseBlackPanel() {
        _blackPanel_up.DOAnchorPosY(0, _easeDuration);
        _blackPanel_down.DOAnchorPosY(0, _easeDuration);
    }

    public void SettingPanel() {
        // 실제 화면 크기 사용
        _screenWidth = _uniqueResolutions[_currentResolutionIndex].width;
        _screenHeight = _uniqueResolutions[_currentResolutionIndex].height;

        // Main Panel Setting
        _mainPanel.sizeDelta = new Vector2(_screenWidth, _screenHeight);
        _mainPanel.anchoredPosition = new Vector2(0, 0);

        _leftPanel.sizeDelta = new Vector2(_screenWidth, _screenHeight);
        _leftPanel.anchoredPosition = new Vector2(-_screenWidth, 0);

        _rightPanel.sizeDelta = new Vector2(_screenWidth, _screenHeight);
        _rightPanel.anchoredPosition = new Vector2(_screenWidth, 0);

        // Black Panel Setting
        _blackPanel_up.sizeDelta = new Vector2(0, _screenHeight * 0.5f);
        _blackPanel_down.sizeDelta = new Vector2(0, _screenHeight * 0.5f);
        _blackPanel_up.anchoredPosition = new Vector2(0, _screenHeight * 0.25f);
        _blackPanel_down.anchoredPosition = new Vector2(0, -_screenHeight * 0.25f);
    }

    public void SetBlackPanel(){
        _blackPanel_up.anchoredPosition = new Vector2(0, 0);
        _blackPanel_down.anchoredPosition = new Vector2(0, 0);
        _blackPanel_up.DOAnchorPosY(_screenHeight * 0.5f, _easeDuration).SetEase(_easeMode);
        _blackPanel_down.DOAnchorPosY(-_screenHeight * 0.5f, _easeDuration).SetEase(_easeMode);
    }

    void Update() {
        // 인트로가 플레이 중일 때만 키 입력 체크
        if (_isIntroPlaying && !_hasSkipped) {
            CheckIntroSkip();
        }
    }

    private void CheckIntroSkip() {
        // 아무 키나 누르고 있는지 확인 (스페이스바, 엔터, 아무 키나)
        if (Input.anyKey) {
            _keyHoldTime += Time.deltaTime;
            
            // 스킵 아이콘 fill 업데이트
            if (_skipIcon != null) {
                _skipIcon.fillAmount = _keyHoldTime / _requiredHoldTime;
            }
            
            // 1초간 누르면 인트로 스킵
            if (_keyHoldTime >= _requiredHoldTime) {
                SkipIntro();
            }
        } else {
            // 키를 놓으면 타이머 리셋
            _keyHoldTime = 0f;
            // 스킵 아이콘 fill 리셋
            if (_skipIcon != null) {
                _skipIcon.fillAmount = 0f;
            }
        }
    }

    private void SkipIntro() {
        if (_hasSkipped) return; // 이미 스킵했으면 중복 실행 방지
        
        _hasSkipped = true;
        _isIntroPlaying = false;
        
        // 스킵 아이콘을 꽉 찬 상태로 유지
        if (_skipIcon != null) {
            _skipIcon.fillAmount = 1f;
        }
        
        // 인트로 디렉터 정지
        _introDirector.Stop();
        
        // 게임 씬으로 이동
        CloseBlackPanel();
        Invoke("LoadGameScene", 1);
    }
}
