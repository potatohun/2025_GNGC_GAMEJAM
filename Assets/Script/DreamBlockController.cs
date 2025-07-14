using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DreamBlockController : BlockController
{
    [Header("Dream Block Options")]
    [SerializeField] private float _floatingSpeed = 5f;
    [SerializeField] private bool _isFloating = false;

    [SerializeField] private int _collisionCount = 0;
    [SerializeField] private int _dreamBlockCount = 1;

    [SerializeField] List<GameObject> _collidedBlockList = new List<GameObject>();

    public ParticleSystem _floatingParticle;
    private PolygonCollider2D _polygonCollider;

    protected override void Awake() {
        base.Awake();

        _polygonCollider = this.GetComponent<PolygonCollider2D>();
    }

    protected override void Update() {
        base.Update();

        if(_isFloating) {
            _rigidbody.velocity = new Vector2(0, _floatingSpeed);
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision) {
        if(collision.gameObject.CompareTag("RealityBlock")) {
            if(!_collidedBlockList.Contains(collision.gameObject))
                _collidedBlockList.Add(collision.gameObject);

            if(_isFloating) {
                SetFloatingStop();
            }
        }

        // 컨트롤 상태 일 때만 상태 전환 처리
        base.OnCollisionEnter2D(collision);
    }

    void OnCollisionExit2D(Collision2D collision) {
        if(collision.gameObject.CompareTag("RealityBlock")) {
            if(_collidedBlockList.Contains(collision.gameObject))
                _collidedBlockList.Remove(collision.gameObject);

                if(_collidedBlockList.Count == 0) {
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
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rigidbody.velocity = Vector2.zero;
        _rigidbody.gravityScale = 0f;

        _floatingParticle.Play();
    }
    
    private void SetFloatingStop() {
        _isFloating = false;
         _rigidbody.bodyType = RigidbodyType2D.Static;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;

        _floatingParticle.Stop();
    }

    public override void FixBlock() {
        base.FixBlock();
        _isFloating = false;
        _floatingParticle.Stop();
        _rigidbody.bodyType = RigidbodyType2D.Static;
    }

    public bool GetFloating() {
        return _isFloating;
    }
}
