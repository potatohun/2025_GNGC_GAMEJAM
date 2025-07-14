using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [SerializeField] private CameraController _cameraController;
    [SerializeField] private Transform _lookAtTarget;
    [SerializeField] private int _level = 1;
    [SerializeField] private int _MoveOffset = 30;
    [SerializeField] private float _moveDuration = 3f;
    
    [SerializeField] private bool _canLevelUp = true;

    private BoxCollider2D _boxCollider;

    void Awake() {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        _cameraController = this.GetComponentInChildren<CameraController>();
        _boxCollider = this.GetComponent<BoxCollider2D>();
    }

    void LevelUp() {
        // 블럭 매니저 정지
        BlockManager.Instance.SetCanSpawn(false);
        BlockManager.Instance.FixAllBlock();

        // 레벨 업
        _level++;
        SoundManager.Instance.PlaySound("LevelUp");

        // 카메라 이동
        float targetY = _lookAtTarget.position.y + _MoveOffset;
        _lookAtTarget.DOMoveY(targetY, _moveDuration).OnComplete(()=>
        {
            _boxCollider.offset += new Vector2(0, _MoveOffset);
            BlockManager.Instance.SetCanSpawn(true);
            BlockManager.Instance.SpawnBlock();
            _canLevelUp = true;
        });
    }

    void OnTriggerStay2D(Collider2D collision) {
        if(_canLevelUp == false)
            return;

        if(collision.gameObject.CompareTag("RealityBlock")) {
            BlockController blockController = collision.gameObject.GetComponent<BlockController>();
            if(blockController == null)
                Debug.Log("blockController is null");

            if(blockController.GetIsControl() == false) {
                if(_canLevelUp) {
                    _canLevelUp = false;
                    LevelUp();
                }
            }
        }
    }
}
