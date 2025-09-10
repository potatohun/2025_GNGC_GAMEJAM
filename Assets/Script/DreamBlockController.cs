using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DreamBlockController : BlockController
{
    [Header("Dream Block Options")]
    [SerializeField] private float _currentFloatingSpeed = 0.1f;
    [SerializeField] private float _maxFloatingSpeed = 5f;
    [SerializeField] private bool _isFloating = false;

    [SerializeField] List<GameObject> _collidedBlockList = new List<GameObject>();

    public ParticleSystem _floatingParticle;
    private PolygonCollider2D _polygonCollider;

    protected override void Awake() {
        base.Awake();

        _polygonCollider = this.GetComponent<PolygonCollider2D>();
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        if(_isFloating) {
            // 최대 속도 값을 초과하지 않도록 조절
            if(_currentFloatingSpeed < _maxFloatingSpeed) {
                _currentFloatingSpeed += Time.deltaTime;
            }

            _rigidbody.velocity = new Vector2(0, _currentFloatingSpeed);
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision) {
        if(_isFixed)
            return;

        // 현실 블록과 충돌했는지 확인
        if(collision.gameObject.CompareTag("RealityBlock") || collision.gameObject.CompareTag("DreamBlock")) {
            if(!_collidedBlockList.Contains(collision.gameObject)) {
                _collidedBlockList.Add(collision.gameObject);
                
                // 떠있는 블록이면 멈춤
                if(_isFloating) {
                    SetFloatingStop();
                }
            }
        }

        // 컨트롤 상태 일 때만 상태 전환 처리
        base.OnCollisionEnter2D(collision);
    }

    void OnCollisionExit2D(Collision2D collision) {
        if(_isFixed)
            return;

        // 현실 블록과의 충돌이 끝났는지 확인
        if(collision.gameObject.CompareTag("RealityBlock") || collision.gameObject.CompareTag("DreamBlock")) {
            if(_collidedBlockList.Contains(collision.gameObject)) {
                _collidedBlockList.Remove(collision.gameObject);

                // 모든 현실 블록과의 충돌이 끝났으면 떠오르기 시작
                if(_collidedBlockList.Count == 0) {
                    SetFloatingStart();
                }
            }
        }
    }

    public override void SetStop() {
        base.SetStop();

        SetFloatingStart();
    }

    private void SetFloatingStart() {
        _isFloating = true;

        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rigidbody.velocity = Vector2.zero;
        _currentFloatingSpeed = 0.5f;
        _rigidbody.gravityScale = 0f;

        _floatingParticle.Play();
    }

    private void SetFloatingStop() {
        _isFloating = false;

        _rigidbody.velocity = Vector2.zero;
        _currentFloatingSpeed = 0;
        _rigidbody.bodyType = RigidbodyType2D.Static;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;

        _floatingParticle.Stop();
    }

    public override void FixBlock() {
        base.FixBlock();

        _isFloating = false;
        _floatingParticle.Stop();
    }

    public bool GetFloating() {
        return _isFloating;
    }
}
