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
    [SerializeField] protected float _rotationSpeed = 360f;
    [SerializeField] protected float _holdTimeRequired = 0f; // 회전을 시작하기 위한 홀드 시간
    [SerializeField] protected float _snapPauseTime = 0.1f; // 각도별 멈춤 시간
    [SerializeField] protected float _snapThreshold = 5f; // 스냅 범위 (도)

    [Header("Block Status")]
    [SerializeField] protected bool _isFalling = true;
    [SerializeField] protected bool _isControl = true;
    [SerializeField] protected bool _isFixed = false;

    [Header("Rotation Status")]
    protected float _currentRotation = 0f; // 현재 회전 각도
    protected float _qHoldTime = 0f; // Q키 홀드 시간
    protected float _eHoldTime = 0f; // E키 홀드 시간
    // protected bool _isRotating = false; // 현재 회전 중인지 (사용하지 않음)
    protected bool _isPaused = false; // 각도별 멈춤 상태인지
    protected float _pauseTimer = 0f; // 멈춤 타이머
    protected int _lastSnapAngle = 0; // 마지막 스냅 각도

    public ParticleSystem _collisionEffect;

    protected Rigidbody2D _rigidbody;
    protected PolygonCollider2D _collider;

    protected UnityEvent _onCollisionEnter = new UnityEvent();


    protected virtual void Awake() {
        // 초기화
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<PolygonCollider2D>();
        
        // 실제 transform의 회전 각도로 _currentRotation 초기화
        _currentRotation = transform.eulerAngles.z;
        _lastSnapAngle = GetCurrentSnapAngle();
        
        // 초기 투명도 설정 (컨트롤 상태일 때 투명)
        UpdateBlockTransparency();
    }

    protected virtual void Start() {
        _rigidbody.gravityScale = 1f;
    }

    protected virtual void FixedUpdate() {
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
            
            // 새로운 회전 시스템 처리
            HandleRotation();
        }
    }

    /// <summary>
    /// 회전 입력을 처리하는 메인 메서드
    /// </summary>
    protected virtual void HandleRotation()
    {
        // 멈춤 상태일 때 타이머 업데이트
        if (_isPaused)
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer <= 0f)
            {
                _isPaused = false;
            }
            return;
        }

        bool qPressed = Input.GetKey(KeyCode.Q);
        bool ePressed = Input.GetKey(KeyCode.E);

        // 홀드 시간이 0이면 즉시 회전, 아니면 홀드 시간 체크
        bool canRotateQ = _holdTimeRequired <= 0 ? qPressed : (qPressed && _qHoldTime >= _holdTimeRequired);
        bool canRotateE = _holdTimeRequired <= 0 ? ePressed : (ePressed && _eHoldTime >= _holdTimeRequired);

        // Q키 홀드 시간 업데이트
        if (qPressed)
        {
            _qHoldTime += Time.deltaTime;
        }
        else
        {
            _qHoldTime = 0f;
        }

        // E키 홀드 시간 업데이트
        if (ePressed)
        {
            _eHoldTime += Time.deltaTime;
        }
        else
        {
            _eHoldTime = 0f;
        }

        // 회전 실행
        if (canRotateQ || canRotateE)
        {
            float rotationAmount = _rotationSpeed * Time.deltaTime;
            
            if (canRotateQ)
            {
                RotateBlock(rotationAmount); // 시계 방향
            }
            else if (canRotateE)
            {
                RotateBlock(-rotationAmount); // 반시계 방향
            }
        }
    }

    /// <summary>
    /// 블록을 회전시키고 각도별 멈춤을 처리
    /// </summary>
    protected virtual void RotateBlock(float rotationAmount)
    {
        _currentRotation += rotationAmount;
        transform.Rotate(0, 0, rotationAmount);

        // 360도로 정규화
        if (_currentRotation >= 360f)
            _currentRotation -= 360f;
        else if (_currentRotation < 0f)
            _currentRotation += 360f;

        // 스냅 각도 확인 (범위 내에서만)
        int currentSnapAngle = GetCurrentSnapAngle();
        if (currentSnapAngle != -1 && currentSnapAngle != _lastSnapAngle)
        {
            _lastSnapAngle = currentSnapAngle;
            StartPause();
        }
    }

    /// <summary>
    /// 현재 각도에서 가장 가까운 스냅 각도를 반환 (범위 제한)
    /// </summary>
    protected virtual int GetCurrentSnapAngle()
    {
        float normalizedAngle = _currentRotation;
        if (normalizedAngle < 0) normalizedAngle += 360f;
        
        // 스냅 가능한 각도들 (90도 간격)
        int[] snapAngles = { 0, 90, 180, 270, 360};
        int closestAngle = -1; // -1은 스냅하지 않음을 의미
        float minDistance = float.MaxValue;

        foreach (int angle in snapAngles)
        {
            float distance = Mathf.Abs(normalizedAngle - angle);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestAngle = angle;
            }
        }

        // 스냅 범위 내에 있을 때만 스냅 각도 반환
        if (minDistance <= _snapThreshold)
        {
            return closestAngle;
        }
        else
        {
            return -1; // 스냅하지 않음
        }
    }

    /// <summary>
    /// 각도별 멈춤 시작 및 각도 보정
    /// </summary>
    protected virtual void StartPause()
    {
        // 각도를 정확한 스냅 각도로 보정
        SnapToClosestAngle();
        
        _isPaused = true;
        _pauseTimer = _snapPauseTime;
    }

    /// <summary>
    /// 현재 각도를 가장 가까운 스냅 각도로 보정
    /// </summary>
    protected virtual void SnapToClosestAngle()
    {
        int targetAngle = GetCurrentSnapAngle();
        
        // 목표 각도로 정확히 회전
        Vector3 currentEulerAngles = transform.eulerAngles;
        currentEulerAngles.z = targetAngle;
        transform.eulerAngles = currentEulerAngles;
        
        // 내부 각도 추적도 업데이트
        _currentRotation = targetAngle;
    }

    /// <summary>
    /// 블록의 투명도를 상태에 따라 업데이트합니다.
    /// </summary>
    private void UpdateBlockTransparency()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (_isFixed)
            {
                // 고정된 블록은 회색
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            else if (_isControl)
            {
                // 컨트롤 상태일 때 반투명
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                // 컨트롤이 끝나면 완전 불투명
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision) {
        if(_isControl == false)
            return;

        _isControl = false;

        _rigidbody.velocity = Vector2.zero;
        Rigidbody2D collision_rigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
        if(collision_rigidbody != null) {
            collision_rigidbody.velocity = Vector2.zero;
        }

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
        // 조작 상태
        _isControl = isControl;
        
        // 투명도 업데이트
        UpdateBlockTransparency();
    }

    public void SetFall() {
        // 떨어지기 상태 (기본)
        _isFalling = true;
    }

    public virtual void SetStop() {
        // 떨어지기 멈춤 상태
        _isFalling = false;
        _isControl = false;
        
        // 콜백 호출
        if(_onCollisionEnter == null)
            return;

        _onCollisionEnter.Invoke();
        _onCollisionEnter = null;
        
        // 컨트롤이 끝났으므로 불투명하고 속도 제어
        UpdateBlockTransparency();
        _rigidbody.velocity = Vector2.zero;
    }

    public void SetOnCollisionEnter(UnityAction action) {
        _onCollisionEnter.AddListener(action);
    }

    public virtual void FixBlock() {
        // 모든 물리 연산 종료
        _isControl = false;
        _isFalling = false;
        _isFixed = true;

        this.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
        _rigidbody.bodyType = RigidbodyType2D.Static;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        
        // 투명도 업데이트 (고정된 블록은 회색으로)
        UpdateBlockTransparency();
    }
    public bool GetIsControl() {
        return _isControl;
    }

    public bool GetIsFalling() {
        return _isFalling;
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

        this.GetComponent<SpriteRenderer>().DOFade(0, 0.5f).OnComplete(() => {
            this.gameObject.SetActive(false);
        });
    }
}
