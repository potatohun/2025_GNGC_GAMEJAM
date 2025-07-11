using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    [Header("Block Prefab")]
    [SerializeField] private GameObject[] _realityBlockPrefab;
    [SerializeField] private GameObject[] _dreamBlockPrefab;

    [Header("Block Spawn Settings")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _canSpawn = true; // 블럭 생성 가능 여부
    [SerializeField] [Range(0, 1)] private float _spawnRatio = 0.5f;
    
    [Header("Spawned Block List")]
    [SerializeField] private List<Transform> _spawnedBlockList = new List<Transform>();

    private CameraController _cameraController;
    private BlockController _currentBlockController;

    void Awake() {
        _cameraController = this.GetComponentInChildren<CameraController>();
    }
    
    void Start() {
        // 게임 시작
        SpawnBlock();
    }

    void Update() {
        if(_spawnedBlockList.Count == 0)
            return;

        // 생성 된 블럭들 중 가장 높은 Y 값 블럭 찾기
        Transform maxYBlock = null;
        foreach(Transform block in _spawnedBlockList) {
            if(maxYBlock == null) {
                maxYBlock = block;
            }
            else if(block.position.y > maxYBlock.position.y) {
                maxYBlock = block;
            }
        }

        _cameraController.SetTarget(maxYBlock);
    }

    private void SpawnBlock() {
        if(_canSpawn == false)
            return;

        // 랜덤 블럭 선택
        GameObject block = null;
        float randomValue = Random.Range(0, 1f);

        if(randomValue < _spawnRatio) {
            // 리얼리티 블럭 생성
            int randomIndex = Random.Range(0, _realityBlockPrefab.Length);
            block = Instantiate(_realityBlockPrefab[randomIndex], _spawnPoint.position, Quaternion.identity);
        }
        else {
            // 드림 블럭 생성
            int randomIndex = Random.Range(0, _dreamBlockPrefab.Length);
            block = Instantiate(_dreamBlockPrefab[randomIndex], _spawnPoint.position, Quaternion.identity);
        }

        // 블럭 초기화
        if(block == null)
            return;

        BlockController blockController = block.GetComponent<BlockController>();
        _currentBlockController = blockController;
        _currentBlockController.SetOnCollisionEnter(OnBlockCollision);
    }

    public void OnBlockCollision() {
        // 다시 생성 가능하도록 설정
        _canSpawn = true; 
        _spawnedBlockList.Add(_currentBlockController.transform);
        _currentBlockController = null;

        SpawnBlock();
    }
}
