using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DreamBlockController : BlockController
{
    [Header("Dream Block Options")]
    [SerializeField] private float _floatingSpeed = 5f;
    [SerializeField] private bool _isFloating = false;

    private int _collisionCount = 0;
    
    private TMP_Text _collisionCountText;

    protected override void Awake() {
        base.Awake();

        _collisionCountText = GetComponentInChildren<TMP_Text>();
        _collisionCountText.text = _collisionCount.ToString();
    }

    protected override void Update() {
        base.Update();

        if(_isFloating) {
            _rigidbody.velocity = new Vector2(0, _floatingSpeed);
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision) {
        // 꿈 블럭끼리 부딪히면 병합 후 SetStop 호출
        if (collision.gameObject.CompareTag("DreamBlock")) {
            // 병합
            SetFloatingStop();
        }
        
        // 떠오르는 블럭 충돌 처리
        if(_isFloating) {
            if (collision.gameObject.CompareTag("RealityBlock")) {
                UpdateCollisionCount(1);
                if(_collisionCount >= 1) {
                    SetFloatingStop();
                }
            }
        }

        // 컨트롤 상태 일 때만 상태 전환 처리
        base.OnCollisionEnter2D(collision);
    }

    void OnCollisionExit2D(Collision2D collision) {
        if(collision.gameObject.CompareTag("RealityBlock")) {
            UpdateCollisionCount(-1);
            if(_collisionCount <= 0) {
                SetFloatingStart();
            }
        }
    }

    protected override void SetStop() {
        base.SetStop();

        SetFloatingStart();
    }
    
    private void SetFloatingStart() {
        _isFloating = true;
        _rigidbody.gravityScale = 0f;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전만 고정
    }
    
    private void SetFloatingStop() {
        _rigidbody.velocity = Vector2.zero;
        _isFloating = false;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll; // 완전 고정
    }

    private void UpdateCollisionCount(int value) {
        _collisionCount += value;
        if(_collisionCount < 0)
            _collisionCount = 0;

        _collisionCountText.text = _collisionCount.ToString();
    }
}
