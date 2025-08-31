using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

        
    [SerializeField] private Transform _lookAtTarget;
    [SerializeField] private int _level = 1;
    [SerializeField] private int _MoveOffset = 30;
    [SerializeField] private float _moveDuration = 3f;
    
    [SerializeField] private bool _canLevelUp = true;

    private BoxCollider2D _boxCollider;
    private CameraController _cameraController;
    private SpawnPointController _spawnPointController;
    void Awake() {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        _boxCollider = this.GetComponent<BoxCollider2D>();
        _cameraController = this.GetComponentInChildren<CameraController>();
        _spawnPointController = this.GetComponentInChildren<SpawnPointController>();
    }

    void LevelUp(Vector3 highestBlockPosition) {
        // 블럭 매니저 정지
        BlockManager.Instance.SetCanSpawn(false);
        BlockManager.Instance.FixAllBlocks();

        // 레벨 업
        _level++;
        SoundManager.Instance.PlaySound("LevelUp");

        // 아이템 생성 (카메라 이동 전)
        if (ItemManager.Instance != null)
        {
            //ItemManager.Instance.SpawnItemOnLevelUp(highestBlockPosition);
            ItemManager.Instance.CheckGetNewItem(highestBlockPosition);
        }
        else
        {
            Debug.LogWarning("ItemManager.Instance is null. Cannot spawn items.");
        }

        // 카메라 이동
        float targetY = _lookAtTarget.position.y + _MoveOffset;
        _lookAtTarget.DOMoveY(targetY, _moveDuration).OnComplete(()=>
        {
            _boxCollider.offset += new Vector2(0, _MoveOffset);
            BlockManager.Instance.SetCanSpawn(true);
            BlockManager.Instance.SpawnBlock();
            _spawnPointController.UpdateMoveSetting(_level);
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

            if(blockController.GetIsControl() == false && blockController.GetIsFixed() == false) {
                if(_canLevelUp) {
                    _canLevelUp = false;
                    LevelUp(collision.transform.position);
                }
            }
        }
    }

    public void RocketItemEffect() {
        _canLevelUp = false;

        // 블럭 매니저 정지
        BlockManager.Instance.SetCanSpawn(false);
        BlockManager.Instance.FixAllBlocks();

        // 레벨 업
        _level += 4;

        // 카메라 이동
        float targetY = _lookAtTarget.position.y + _MoveOffset * 4;
        _lookAtTarget.DOMoveY(targetY, _moveDuration * 2).SetEase(Ease.InCirc).OnComplete(()=>
        {
            _boxCollider.offset += new Vector2(0, _MoveOffset * 4);
            BlockManager.Instance.SetCanSpawn(true);
            BlockManager.Instance.SpawnBlock();
            _spawnPointController.UpdateMoveSetting(_level);
            _canLevelUp = true;
        });
    }

    public Transform GetLookAtTarget() {
        return _lookAtTarget;
    }
}
