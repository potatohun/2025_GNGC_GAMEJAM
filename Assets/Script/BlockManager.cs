using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockManager : MonoBehaviour
{
    public static BlockManager Instance;

    [Header("Block Prefab")]
    [SerializeField] private GameObject[] _realityBlockPrefab;
    [SerializeField] private GameObject[] _dreamBlockPrefab;

    [Header("Block Spawn Settings")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _canSpawn = true; // 블럭 생성 가능 여부
    [SerializeField] [Range(0, 1)] private float _spawnRatio = 0.5f;
    [SerializeField] private float _spawnDelay = 0.5f;
    [SerializeField] private float _maxYBlockPosition = 0;
    [SerializeField] private Image _nextBlockImage;
    
    [Header("Spawned Block List")]
    [SerializeField] private List<Transform> _spawnedRealityBlockList = new List<Transform>();
    [SerializeField] private List<Transform> _spawnedDreamBlockList = new List<Transform>();
    [SerializeField] private List<Transform> _targetBlockList = new List<Transform>();

    private BlockController _currentBlockController;
    private bool _isFirstSpawn = true; // 첫 번째 생성 여부
    
    // 다음 블록 정보
    private GameObject _nextBlockPrefab;
    private bool _isNextBlockReality = true;
    

    void Awake() {
        if(Instance == null)
            Instance = this;    
        else
            Destroy(this.gameObject);
    }

    void Start() {
        Invoke("StartGame", 5f);
    }

    public void StartGame() {
        // 다음 블록 결정 및 이미지 업데이트
        DetermineNextBlock();
        UpdateNextBlockImage();
        
        // 게임 시작
        SpawnBlock();
    }

    void Update() {
        if(_targetBlockList.Count == 0)
            return;

        // 생성 된 블럭들 중 가장 높은 Y 값 블럭 찾기
        float _maxYTmp = -1;
        foreach(Transform block in _targetBlockList) {
            if(block.position.y > _maxYTmp) {
                _maxYTmp = block.position.y;
            }
        }

        // 최고 높이가 갱신이 되었으면 게임매니저에 알림
        if(_maxYTmp > _maxYBlockPosition) {
            _maxYBlockPosition = _maxYTmp;
            GameManager.Instance.SetMaxHeight(_maxYBlockPosition);
        }
    }

    public void SpawnBlock() {
        if(_canSpawn == false)
            return;

        // 결정된 다음 블록 생성
        GameObject block = null;
        
        if (_isFirstSpawn) {
            // 첫 번째 생성이면 무조건 현실 블럭
            block = Instantiate(_nextBlockPrefab, _spawnPoint.position, Quaternion.identity);
            _spawnedRealityBlockList.Add(block.transform);
            _isFirstSpawn = false; // 첫 번째 생성 완료
        } else {
            // 결정된 다음 블록 생성
            if (_isNextBlockReality) {
                block = Instantiate(_nextBlockPrefab, _spawnPoint.position, Quaternion.identity);
                _spawnedRealityBlockList.Add(block.transform);
            } else {
                block = Instantiate(_nextBlockPrefab, _spawnPoint.position, Quaternion.identity);
                _spawnedDreamBlockList.Add(block.transform);
            }
        }

        // 블럭 초기화
        if(block == null)
            return;

        BlockController blockController = block.GetComponent<BlockController>();
        _currentBlockController = blockController;
        _currentBlockController.SetOnCollisionEnter(OnBlockCollision);
        
        // 다음 블록 결정 및 이미지 업데이트
        DetermineNextBlock();
        UpdateNextBlockImage();
    }

    public void OnBlockCollision() {
        // 다시 생성 가능하도록 설정
        _canSpawn = true; 
        if(_currentBlockController.gameObject.CompareTag("RealityBlock")) {
            if(_currentBlockController.GetIsFixed() == false) {
                _targetBlockList.Add(_currentBlockController.transform);
            }
        }

        _currentBlockController = null;
        Invoke("SpawnBlock", _spawnDelay);
    }

    public void FixAllBlock() {
        foreach(Transform block in _spawnedRealityBlockList) {
            BlockController blockController = block.GetComponent<BlockController>();
            blockController.FixBlock();
            blockController.enabled = false;
        }

        foreach(Transform block in _spawnedDreamBlockList) {
            BlockController blockController = block.GetComponent<BlockController>();
            blockController.FixBlock();
            blockController.enabled = false;
        }

        // 리스트 초기화.
        _spawnedRealityBlockList.Clear();
        _spawnedDreamBlockList.Clear();
        _targetBlockList.Clear();
    }

    public void SetCanSpawn(bool canSpawn) {
        _canSpawn = canSpawn;
    }
    
    // 다음 블록 결정
    private void DetermineNextBlock() {
        // 배열이 비어있는지 확인
        if (_realityBlockPrefab == null || _realityBlockPrefab.Length == 0) {
            Debug.LogError("Reality Block Prefab 배열이 비어있습니다!");
            return;
        }
        
        if (_dreamBlockPrefab == null || _dreamBlockPrefab.Length == 0) {
            Debug.LogError("Dream Block Prefab 배열이 비어있습니다!");
            return;
        }
        
        if (_isFirstSpawn) {
            // 첫 번째 블록은 무조건 현실 블록
            int randomIndex = Random.Range(0, _realityBlockPrefab.Length);
            _nextBlockPrefab = _realityBlockPrefab[randomIndex];
            _isNextBlockReality = true;
        } else {
            // 랜덤 선택
            float randomValue = Random.Range(0, 1f);
            
            if (randomValue < _spawnRatio) {
                // 현실 블록
                int randomIndex = Random.Range(0, _realityBlockPrefab.Length);
                _nextBlockPrefab = _realityBlockPrefab[randomIndex];
                _isNextBlockReality = true;
            } else {
                // 드림 블록
                int randomIndex = Random.Range(0, _dreamBlockPrefab.Length);
                _nextBlockPrefab = _dreamBlockPrefab[randomIndex];
                _isNextBlockReality = false;
            }
        }
    }
    
    // 다음 블록 이미지 업데이트
    private void UpdateNextBlockImage() {
        if (_nextBlockImage != null && _nextBlockPrefab != null) {
            SpriteRenderer spriteRenderer = _nextBlockPrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null) {
                _nextBlockImage.sprite = spriteRenderer.sprite;
                _nextBlockImage.SetNativeSize();
            }
        }
    }

    public void EndGame() {
        _canSpawn = false;
        
        FixAllBlock();

        _currentBlockController = null; 

        _spawnDelay = 100000f;
    }
}
