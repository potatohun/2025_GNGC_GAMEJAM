using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class BlockController : MonoBehaviour
{
    [Header("Block Options")]
    [SerializeField] protected float _fallSpeed = 5f;
    [SerializeField] protected float _moveSpeed = 10f;

    [Header("Block Status")]
    [SerializeField] protected bool _isFalling = true;
    [SerializeField] protected bool _isControl = true;
    [SerializeField] protected bool _isFixed = false;

    public ParticleSystem _collisionEffect;

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
            float moveInputHorizontal = Input.GetAxis("Horizontal");
            float moveInputVertical = Input.GetAxis("Vertical");
            if(moveInputVertical < 0)
            {
                _rigidbody.velocity = new Vector2(moveInputHorizontal * _moveSpeed, 2 * -_fallSpeed);
            }
            else {
                _rigidbody.velocity = new Vector2(moveInputHorizontal * _moveSpeed, -_fallSpeed);
            }
            
            // Q와 E를 이용한 Z축 회전
            if (Input.GetKey(KeyCode.Q)) {
                // Q키: -Z 방향 회전 (반시계 방향)
                transform.Rotate(0, 0, 180f * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.E)) {
                // E키: +Z 방향 회전 (시계 방향)
                transform.Rotate(0, 0, -180f * Time.deltaTime);
            }
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision) {
        if(_isControl == false)
            return;

        int contactCount = collision.contactCount;
        ContactPoint2D contact = collision.contacts[contactCount - 1];
        Vector2 pos = contact.point;

        _collisionEffect.transform.position = pos;
        _collisionEffect.Play();
        SoundManager.Instance.PlaySound("SetBlock");      

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

        _onCollisionEnter = null;
    }

    public void SetOnCollisionEnter(UnityAction action) {
        _onCollisionEnter.AddListener(action);
    }

    public virtual void FixBlock() {
        // 모든 물리 연산 종료
        _isControl = false;
        _isFalling = false;
        _isFixed = true;

        _rigidbody.bodyType = RigidbodyType2D.Static;
        this.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
    }
    public bool GetIsControl() {
        return _isControl;
    }

    public bool GetIsFixed() {
        return _isFixed;
    }

    public void TriggerTrap() {
        if(_isFixed)
            return;

        _isFixed = true;
        _collider.enabled = false;

        SetStop();

        GameManager.Instance.Damage();
        SoundManager.Instance.PlaySound("TrapTrigger");

        this.GetComponent<SpriteRenderer>().DOFade(0, 0.5f).OnComplete(() => {
            this.gameObject.SetActive(false);
        });
    }
}
