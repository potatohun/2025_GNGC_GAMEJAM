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

    public RectTransform _nextItemNoti;

    public GameObject _nextItemGameObject;

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

        // 최대 점수 이미지 채우기
        _highestScoreImage.fillAmount = _highestScore / MAX_SCORE;
        _currentScoreImage.fillAmount = _currentScore / MAX_SCORE;
        
        // 다음 아이템 위치 noti 위치 설정
        UpdateNextItemNotiPosition();
    }
    
    private void UpdateNextItemNotiPosition()
    {
        float next_item_height = ItemManager.Instance.GetNextItemHeight();
        if (next_item_height == -1f) {
            _nextItemGameObject.SetActive(false);
            return;
        }
        
        // fillAmount에 따른 Y 위치 계산 (주어진 값 기준)
        float targetY = (next_item_height/ MAX_SCORE - 0.5f) * 506f;
        
        // 기존 tween이 실행 중이면 종료
        if (_notiMoveTween != null)
            _notiMoveTween.Kill();
        
        _nextItemGameObject.SetActive(true);
        // DOTween을 사용하여 부드럽게 이동
        _notiMoveTween = _nextItemNoti.DOAnchorPosY(targetY, 0.5f).OnComplete(() => {
            // X 위치도 함께 설정 (이동 중에는 Y만 변경)
            _nextItemNoti.anchoredPosition = new Vector2(-80f, targetY);
            _notiMoveTween = null;
        });
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
            
            // 최고 점수 이미지를 DOTween으로 부드럽게 업데이트
            float targetFillAmount = _currentScore / MAX_SCORE;
            
            // 기존 fillAmount tween이 실행 중이면 종료
            if (_fillAmountTween != null)
                _fillAmountTween.Kill();
            
            _fillAmountTween = _highestScoreImage.DOFillAmount(targetFillAmount, 0.5f).OnComplete(() => {
                // fillAmount 변경 완료 후 noti 위치 업데이트
                UpdateNextItemNotiPosition();
                _fillAmountTween = null;
            });
        }
    }
}
