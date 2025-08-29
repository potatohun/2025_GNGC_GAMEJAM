using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

public class ProgressManager : MonoBehaviour
{
    const float MAX_SCORE = 1000;

    public static ProgressManager Instance;

    public Image _highestScoreImage;
    public Image _currentScoreImage;

    public RectTransform _highestScoreNoti;

    public GameObject _highestScoreGameObject;
    public GameObject _newHighestScoreGameObject;

    public RectTransform _nextItemNoti;

    [SerializeField] private float _highestScore;
    [SerializeField] private float _currentScore;

    private bool _isNewRecord = false;
    
    // DOTween 참조를 위한 변수
    private Tween _notiMoveTween;
    private Tween _fillAmountTween;

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
    
    private void Start()
    {
        _isNewRecord = false;

        _highestScore = DataManager.Instance.GetHighScore();
        _currentScore = 0;

        // noti 갱신
        if(_currentScore >= _highestScore)
        {
            _highestScoreGameObject.SetActive(false);
            _newHighestScoreGameObject.SetActive(true);
        } else {
            _highestScoreGameObject.SetActive(true);
            _newHighestScoreGameObject.SetActive(false);
        }

        // 최대 점수 이미지 채우기
        _highestScoreImage.fillAmount = _highestScore / MAX_SCORE;
        _currentScoreImage.fillAmount = _currentScore / MAX_SCORE;
        
        // 최고 점수 noti 위치 설정
        UpdateHighestScoreNotiPosition();
    }
    
    private void UpdateHighestScoreNotiPosition()
    {
        if (_highestScoreImage != null && _highestScoreNoti != null)
        {
            // Image의 RectTransform 가져오기
            RectTransform imageRect = _highestScoreImage.GetComponent<RectTransform>();
            
            // fillAmount에 따른 Y 위치 계산 (주어진 값 기준)
            float targetY = (_highestScoreImage.fillAmount - 0.5f) * 506f;
            
            // 기존 tween이 실행 중이면 종료
            if (_notiMoveTween != null)
                _notiMoveTween.Kill();
            
            // DOTween을 사용하여 부드럽게 이동
            _notiMoveTween = _highestScoreNoti.DOAnchorPosY(targetY, 0.5f).OnComplete(() => {
                // X 위치도 함께 설정 (이동 중에는 Y만 변경)
                _highestScoreNoti.anchoredPosition = new Vector2(
                    imageRect.anchoredPosition.x - 65f,
                    targetY
                );
                _notiMoveTween = null;
            });
        }
    }

    public void SetCurrentScore(float score)
    {
        _currentScore = score;
        _currentScoreImage.fillAmount = _currentScore / MAX_SCORE;

        if(_currentScore > _highestScore)
        {
            // 최고 점수 갱신
            DataManager.Instance.SetHighScore(_currentScore);
            _isNewRecord = true;

            // noti 변경
            _highestScoreGameObject.SetActive(false);
            _newHighestScoreGameObject.SetActive(true);
            
            // 최고 점수 이미지를 DOTween으로 부드럽게 업데이트
            float targetFillAmount = _currentScore / MAX_SCORE;
            
            // 기존 fillAmount tween이 실행 중이면 종료
            if (_fillAmountTween != null)
                _fillAmountTween.Kill();
            
            _fillAmountTween = _highestScoreImage.DOFillAmount(targetFillAmount, 0.5f).OnComplete(() => {
                // fillAmount 변경 완료 후 noti 위치 업데이트
                UpdateHighestScoreNotiPosition();
                _fillAmountTween = null;
            });
        }
    }
}
