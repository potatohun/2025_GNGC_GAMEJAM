using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpawnPointController : MonoBehaviour
{
    [Header("Move Offset Settings")]
    [SerializeField] private float _minMoveOffset = 0f;
    [SerializeField] private float _maxMoveOffset = 30f;
    [SerializeField] private float _increaseMoveOffsetPerLevel = 1f;

    [Header("Move Duration Settings")]
    [SerializeField] private float _minMoveDuration = 1f;
    [SerializeField] private float _maxMoveDuration = 10f;
    [SerializeField] private float _decreaseMoveDurationPerLevel = 0.1f;

    [Header("Move Ease Settings")]
    [SerializeField] private Ease _moveEase = Ease.Linear;
    
    private float _startPositionX;
    private float _leftPosition;
    private float _rightPosition;

    private float _currentMoveOffset;
    private float _currentMoveDuration;
    
    private Sequence _moveSequence;
    
    void Start()
    {
        // Set Move Offset
        _currentMoveOffset = _minMoveOffset;

        // Set Move Duration
        _currentMoveDuration = _maxMoveDuration;

        // Set Start Position
        _startPositionX = transform.position.x;
        _leftPosition = _startPositionX - _currentMoveOffset;
        _rightPosition = _startPositionX + _currentMoveOffset;
        
        // Start Moving
        StartMoving();
    }

    public void UpdateMoveSetting (int level) {
        int targetLevel = level - 1;

        // move offset update
        _currentMoveOffset += _increaseMoveOffsetPerLevel * targetLevel;
        if(_currentMoveOffset > _maxMoveOffset) {
            _currentMoveOffset = _maxMoveOffset;
        }

        // move duration update (너무 빨라져서 보류류)
        // _currentMoveDuration -= _decreaseMoveDurationPerLevel * targetLevel;
        // if(_currentMoveDuration < _minMoveDuration) {
        //     _currentMoveDuration = _minMoveDuration;
        // }

        // move position update
        _leftPosition = _startPositionX - _currentMoveOffset;
        _rightPosition = _startPositionX + _currentMoveOffset;

        // move sequence update
        StartMoving();
    }
    
    void StartMoving()
    {
        // 초기화
        if(_moveSequence != null) {
            _moveSequence.Kill();
            _moveSequence = null;
        }

        transform.position = new Vector3(_startPositionX, transform.position.y, transform.position.z);

        // 왼쪽으로 이동 후 오른쪽으로 이동하는 시퀀스
        _moveSequence = DOTween.Sequence();
        _moveSequence.Append(transform.DOMoveX(_leftPosition, _currentMoveDuration * 0.5f).SetEase(_moveEase));
        _moveSequence.Append(transform.DOMoveX(_rightPosition, _currentMoveDuration).SetEase(_moveEase));
        _moveSequence.Append(transform.DOMoveX(_startPositionX, _currentMoveDuration * 0.5f).SetEase(_moveEase));
        
        // 무한 반복
        _moveSequence.SetLoops(-1, LoopType.Restart);
    }
}
