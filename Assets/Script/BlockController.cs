using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BlockController : MonoBehaviour
{
    [Header("Block Options")]
    [SerializeField] protected float _fallSpeed = 5f;
    [SerializeField] protected float _moveSpeed = 10f;

    [Header("Block Status")]
    [SerializeField] protected bool _isFalling = true;
    [SerializeField] protected bool _isControl = true;

    protected Rigidbody2D _rigidbody;
    protected PolygonCollider2D _collider;

    protected UnityEvent _onCollisionEnter = new UnityEvent();

    protected virtual void Awake() {
        // 초기화
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<PolygonCollider2D>();
    }

    protected virtual void Start() {
        _rigidbody.gravityScale = 1f;
    }

    protected virtual void Update() {
        // 떨어지기
        if(_isFalling) {
            _rigidbody.velocity = new Vector2(0, -_fallSpeed);
        }

        if(_isControl) {
            // 좌우 이동 조작
            float moveInput = Input.GetAxis("Horizontal");
            _rigidbody.velocity = new Vector2(moveInput * _moveSpeed, _rigidbody.velocity.y);
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision) {
        if(_isControl == false)
            return;

        // 컨트롤 상태 일 때만 상태 전환 처리
        SetStop();
    }

    void SetControl(bool isControl) {
        // 조작 상태태
        _isControl = isControl;
    }

    public void SetFall() {
        // 떨어지기 상태 (기본)
        _isFalling = true;
    }

    protected virtual void SetStop() {
        // 떨어지기 멈춤 상태
        _isFalling = false;
        _isControl = false;

        _rigidbody.velocity = Vector2.zero;
        
        // 콜백 호출
        if(_onCollisionEnter == null)
            return;

        _onCollisionEnter.Invoke();
    }

    public void SetOnCollisionEnter(UnityAction action) {
        _onCollisionEnter.AddListener(action);
    }
}
