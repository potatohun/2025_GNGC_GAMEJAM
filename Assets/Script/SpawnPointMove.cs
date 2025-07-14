using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpawnPointMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveDistance = 15f;
    [SerializeField] private float _moveDuration = 2f;
    [SerializeField] private Ease _moveEase = Ease.InOutSine;
    
    private Vector3 _startPosition;
    private float _leftPosition;
    private float _rightPosition;
    
    void Start()
    {
        // 시작 위치 저장
        _startPosition = transform.position;
        
        // 좌우 목표 위치 계산
        _leftPosition = _startPosition.x - _moveDistance;
        _rightPosition = _startPosition.x + _moveDistance;
        
        // 왕복 이동 시작
        StartMoving();
    }
    
    void StartMoving()
    {
        // 왼쪽으로 이동 후 오른쪽으로 이동하는 시퀀스
        Sequence moveSequence = DOTween.Sequence();
        
        // 왼쪽으로 이동
        moveSequence.Append(transform.DOMoveX(_leftPosition, _moveDuration).SetEase(_moveEase));
        
        // 오른쪽으로 이동
        moveSequence.Append(transform.DOMoveX(_rightPosition, _moveDuration).SetEase(_moveEase));

        moveSequence.Append(transform.DOMoveX(_startPosition.x, _moveDuration).SetEase(_moveEase));
        
        // 무한 반복
        moveSequence.SetLoops(-1, LoopType.Restart);
    }
}
