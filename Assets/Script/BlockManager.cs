using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BlockManager : MonoBehaviour
{
    public static BlockManager Instance { get; private set; }

    [Header("Block Prefab")]
    [SerializeField] private GameObject[] _realityBlockPrefabs;
    [SerializeField] private GameObject[] _dreamBlockPrefabs;

    [Header("Block Spawn Settings")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private bool _canSpawn = true;
    [SerializeField] [Range(0, 1)] private float _currentRealityProbability = 0.65f;
    [SerializeField] private float _spawnDelay = 0.5f;
    [SerializeField] private float _gameStartDelay = 5f;
    [SerializeField] private Image _nextBlockImage;
    
    [Header("Spawned Block Lists")]
    [SerializeField] private List<Transform> _spawnedRealityBlocks = new List<Transform>();
    [SerializeField] private List<Transform> _spawnedDreamBlocks = new List<Transform>();
    [SerializeField] private List<Transform> _targetBlocks = new List<Transform>();

    // Private fields
    private BlockController _currentBlockController;
    private bool _isFirstSpawn = true;
    private GameObject _nextBlockPrefab;
    private bool _isNextBlockReality = true;
    private float _maxYBlockPosition = 0f;

    // 블록 반복 방지 시스템
    private int _consecutiveRealityBlocks = 0;  // 연속 현실 블록 수
    private int _consecutiveDreamBlocks = 0;    // 연속 꿈 블록 수
    private const float BASE_REALITY_PROBABILITY = 0.65f;  // 기본 현실 블록 확률
    private const float DREAM_BONUS = 0.15f;  // 꿈 블록 등장 시 현실 블록 확률 증가 (5%)
    private const float REALITY_PENALTY = 0.30f;  // 현실 블록 3연속 시 확률 감소 (7%)

    // 블록 Index 반복 방지 시스템
    private int _lastRealityBlockIndex = -1;  // 마지막 현실 블록 Index
    private int _lastDreamBlockIndex = -1;    // 마지막 꿈 블록 Index
    private const int INVALID_INDEX = -1;     // 유효하지 않은 Index 값

    // 블록 회전 시스템
    private float _nextBlockRotation = 0f;    // 다음 블록의 회전 각도
    private readonly float[] _rotationAngles = { 0f, 90f, 180f, 270f };  // 가능한 회전 각도들

    // Events
    public event System.Action<float> OnMaxHeightChanged;
    public event System.Action<BlockController> OnBlockSpawned;
    public event System.Action<BlockController> OnBlockCollisionEvent;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeSingleton();
        ValidateComponents();
    }

    private void Start()
    {
        StartCoroutine(StartGameWithDelay());
    }

    private void Update()
    {
        UpdateMaxHeight();
    }

    #endregion

    #region Initialization

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple BlockManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    private void ValidateComponents()
    {
        if (_spawnPoint == null)
        {
            Debug.LogError("SpawnPoint is not assigned in BlockManager!");
        }

        if (_realityBlockPrefabs == null || _realityBlockPrefabs.Length == 0)
        {
            Debug.LogError("Reality Block Prefabs array is empty!");
        }

        if (_dreamBlockPrefabs == null || _dreamBlockPrefabs.Length == 0)
        {
            Debug.LogError("Dream Block Prefabs array is empty!");
        }

        if (_nextBlockImage == null)
        {
            Debug.LogWarning("Next Block Image is not assigned. UI preview will be disabled.");
        }
    }

    private IEnumerator StartGameWithDelay()
    {
        yield return new WaitForSeconds(_gameStartDelay);
        StartGame();
    }

    #endregion

    #region Public Methods

    public void StartGame()
    {
        if (!ValidateGameStart())
            return;

        // 확률 시스템 초기화
        ResetProbability();
        
        DetermineNextBlock();
        UpdateNextBlockImage();
        SpawnBlock();
    }

    public void SpawnBlock()
    {
        if (!CanSpawnBlock())
            return;

        GameObject block = CreateBlock();
        if (block == null)
            return;

        InitializeBlock(block);
        UpdateNextBlock();
    }

    public void OnBlockCollision()
    {
        if (_currentBlockController == null)
            return;

        if(_canSpawn == false)
            return;

        HandleBlockCollision();
        ScheduleNextSpawn();
    }

    public void FixAllBlocks()
    {
        // 블록 고정 전 컨트롤 상태 종료
        if(_currentBlockController != null)
            _currentBlockController.SetStop();

        FixBlockList(_spawnedRealityBlocks);
        FixBlockList(_spawnedDreamBlocks);

        ClearAllLists();
    }

    public void FixAllBlocksExceptControlBlock()
    {
        FixBlockListExceptControlBlock(_spawnedRealityBlocks);
        FixBlockListExceptControlBlock(_spawnedDreamBlocks);

        ClearAllListsExceptControlBlock();
    }

    public void SetCanSpawn(bool canSpawn)
    {
        _canSpawn = canSpawn;
    }

    public void EndGame()
    {
        _canSpawn = false;
        FixAllBlocks();
        _currentBlockController = null;
        _spawnDelay = float.MaxValue; // 더 명확한 방법
    }

    // 외부에서 확률 조정을 위한 메서드들
    public void SetRealityBlockProbability(float probability)
    {
        _currentRealityProbability = Mathf.Clamp01(probability);
        Debug.Log($"Reality block probability set to: {_currentRealityProbability:P1}");
    }

    public float GetCurrentRealityProbability()
    {
        return _currentRealityProbability;
    }

    public (int consecutiveReality, int consecutiveDream) GetConsecutiveCounts()
    {
        return (_consecutiveRealityBlocks, _consecutiveDreamBlocks);
    }

    public void ForceNextBlockType(bool isReality)
    {
        if (isReality)
        {
            SetNextRealityBlock();
            UpdateProbabilityAfterRealityBlock();
        }
        else
        {
            SetNextDreamBlock();
            UpdateProbabilityAfterDreamBlock();
        }
        
        Debug.Log($"Forced next block type: {(isReality ? "Reality" : "Dream")}");
    }

    // Index 정보 조회 메서드들
    public (int realityIndex, int dreamIndex) GetLastBlockIndexes()
    {
        return (_lastRealityBlockIndex, _lastDreamBlockIndex);
    }

    public void ResetBlockIndexes()
    {
        _lastRealityBlockIndex = INVALID_INDEX;
        _lastDreamBlockIndex = INVALID_INDEX;
        Debug.Log("Block indexes reset");
    }

    public void ForceNextBlockIndex(bool isReality, int index)
    {
        if (isReality)
        {
            if (index >= 0 && index < _realityBlockPrefabs.Length)
            {
                _lastRealityBlockIndex = index;
                _nextBlockPrefab = _realityBlockPrefabs[index];
                _isNextBlockReality = true;
                Debug.Log($"Forced Reality Block Index: {index}");
            }
            else
            {
                Debug.LogError($"Invalid Reality Block Index: {index}");
            }
        }
        else
        {
            if (index >= 0 && index < _dreamBlockPrefabs.Length)
            {
                _lastDreamBlockIndex = index;
                _nextBlockPrefab = _dreamBlockPrefabs[index];
                _isNextBlockReality = false;
                Debug.Log($"Forced Dream Block Index: {index}");
            }
            else
            {
                Debug.LogError($"Invalid Dream Block Index: {index}");
            }
        }
    }

    // 회전 정보 조회 및 조정 메서드들
    public float GetNextBlockRotation()
    {
        return _nextBlockRotation;
    }

    public void SetNextBlockRotation(float rotation)
    {
        _nextBlockRotation = rotation;
        UpdateNextBlockImage();  // UI 업데이트
        Debug.Log($"Next block rotation set to: {rotation}°");
    }

    public void SetRandomNextBlockRotation()
    {
        SetRandomRotation();
        UpdateNextBlockImage();  // UI 업데이트
        Debug.Log($"Next block rotation randomized to: {_nextBlockRotation}°");
    }

    public float[] GetAvailableRotations()
    {
        return _rotationAngles;
    }

    #endregion

    #region Private Methods

    private bool ValidateGameStart()
    {
        if (_realityBlockPrefabs == null || _realityBlockPrefabs.Length == 0)
        {
            Debug.LogError("Cannot start game: Reality block prefabs are not configured.");
            return false;
        }

        if (_dreamBlockPrefabs == null || _dreamBlockPrefabs.Length == 0)
        {
            Debug.LogError("Cannot start game: Dream block prefabs are not configured.");
            return false;
        }

        return true;
    }

    private bool CanSpawnBlock()
    {
        if (!_canSpawn)
        {
            Debug.Log("Block spawning is currently disabled.");
            return false;
        }

        if (_nextBlockPrefab == null)
        {
            Debug.LogError("Next block prefab is null. Cannot spawn block.");
            return false;
        }

        return true;
    }

    private GameObject CreateBlock()
    {
        if (_spawnPoint == null)
        {
            Debug.LogError("Spawn point is null. Cannot create block.");
            return null;
        }

        // 회전된 Quaternion 생성
        Quaternion rotation = Quaternion.Euler(0f, 0f, _nextBlockRotation);
        GameObject block = Instantiate(_nextBlockPrefab, _spawnPoint.position, rotation);
        
        if (block == null)
        {
            Debug.LogError("Failed to instantiate block prefab.");
            return null;
        }

        AddBlockToList(block);
        return block;
    }

    private void AddBlockToList(GameObject block)
    {
        if (_isFirstSpawn)
        {
            _spawnedRealityBlocks.Add(block.transform);
            _isFirstSpawn = false;
        }
        else
        {
            if (_isNextBlockReality)
            {
                _spawnedRealityBlocks.Add(block.transform);
            }
            else
            {
                _spawnedDreamBlocks.Add(block.transform);
            }
        }
    }

    private void InitializeBlock(GameObject block)
    {
        BlockController blockController = block.GetComponent<BlockController>();
        if (blockController == null)
        {
            Debug.LogError("Block prefab does not have BlockController component.");
            return;
        }

        _currentBlockController = blockController;
        _currentBlockController.SetOnCollisionEnter(OnBlockCollision);
        
        OnBlockSpawned?.Invoke(_currentBlockController);
    }

    private void UpdateNextBlock()
    {
        DetermineNextBlock();
        UpdateNextBlockImage();
    }

    private void HandleBlockCollision()
    {
        _canSpawn = true;
        
        if (_currentBlockController.gameObject.CompareTag("RealityBlock"))
        {
            if (!_currentBlockController.GetIsFixed())
            {
                _targetBlocks.Add(_currentBlockController.transform);
            }
        }

        OnBlockCollisionEvent?.Invoke(_currentBlockController);
        _currentBlockController = null;
    }

    private void ScheduleNextSpawn()
    {
        StartCoroutine(SpawnBlockWithDelay());
    }

    private IEnumerator SpawnBlockWithDelay()
    {
        yield return new WaitForSeconds(_spawnDelay);
        SpawnBlock();
    }

    private void FixBlockList(List<Transform> blockList)
    {
        foreach (Transform block in blockList)
        {
            if (block == null) continue;
            
            BlockController blockController = block.GetComponent<BlockController>();
            if (blockController != null)
            {
                blockController.FixBlock();
                blockController.enabled = false;
            }
        }
    }

    private void FixBlockListExceptControlBlock(List<Transform> blockList)
    {
        foreach (Transform block in blockList)
        {
            if (block == null) continue;
            
            BlockController blockController = block.GetComponent<BlockController>();
            if (blockController != null)
            {
                if (blockController == _currentBlockController)
                    continue;

                blockController.FixBlock();
                blockController.enabled = false;
            }
        }
    }

    private void ClearAllLists()
    {
        _spawnedRealityBlocks.Clear();
        _spawnedDreamBlocks.Clear();
        _targetBlocks.Clear();
    }

    private void ClearAllListsExceptControlBlock()
    {
        // 현재 컨트롤 블록을 제외한 나머지 블록들 clear
        _spawnedRealityBlocks.Clear();
        _spawnedDreamBlocks.Clear();
        
        // 현재 컨트롤 블록이 있다면 다시 추가
        if (_currentBlockController != null)
        {
            if (_currentBlockController.gameObject.CompareTag("RealityBlock"))
            {
                _spawnedRealityBlocks.Add(_currentBlockController.transform);
            }
            else
            {
                _spawnedDreamBlocks.Add(_currentBlockController.transform);
            }
        }
    }


    private void DetermineNextBlock()
    {
        if (_isFirstSpawn)
        {
            SetNextRealityBlock();
        }
        else
        {
            SetRandomNextBlock();
        }
    }

    private void SetNextRealityBlock()
    {
        int randomIndex = GetRandomIndexExcludingLast(_realityBlockPrefabs.Length, _lastRealityBlockIndex);
        _nextBlockPrefab = _realityBlockPrefabs[randomIndex];
        _isNextBlockReality = true;
        
        // 마지막 Index 업데이트
        _lastRealityBlockIndex = randomIndex;
        
        // 랜덤 회전 설정
        SetRandomRotation();
        
        Debug.Log($"Selected Reality Block Index: {randomIndex}, Rotation: {_nextBlockRotation}°");
    }

    private void SetRandomNextBlock()
    {
        // 가중치 조절된 확률로 블록 타입 결정
        float randomValue = Random.Range(0f, 1f);
        
        if (randomValue < _currentRealityProbability)
        {
            SetNextRealityBlock();
            UpdateProbabilityAfterRealityBlock();
        }
        else
        {
            SetNextDreamBlock();
            UpdateProbabilityAfterDreamBlock();
        }
        
        Debug.Log($"Block Type Decision - Reality Prob: {_currentRealityProbability:P1}, " +
                  $"Consecutive Reality: {_consecutiveRealityBlocks}, " +
                  $"Consecutive Dream: {_consecutiveDreamBlocks}");
    }

    private void SetNextDreamBlock()
    {
        int randomIndex = GetRandomIndexExcludingLast(_dreamBlockPrefabs.Length, _lastDreamBlockIndex);
        _nextBlockPrefab = _dreamBlockPrefabs[randomIndex];
        _isNextBlockReality = false;
        
        // 마지막 Index 업데이트
        _lastDreamBlockIndex = randomIndex;
        
        // 랜덤 회전 설정
        SetRandomRotation();
        
        Debug.Log($"Selected Dream Block Index: {randomIndex}, Rotation: {_nextBlockRotation}°");
    }

    private void SetRandomRotation()
    {
        int randomIndex = Random.Range(0, _rotationAngles.Length);
        _nextBlockRotation = _rotationAngles[randomIndex];
    }

    private void UpdateProbabilityAfterRealityBlock()
    {
        _consecutiveRealityBlocks++;
        _consecutiveDreamBlocks = 0;  // 꿈 블록 연속 카운트 리셋
        
        // 현실 블록 3연속 등장 시 확률 감소
        if (_consecutiveRealityBlocks >= 3)
        {
            float penalty = REALITY_PENALTY * (_consecutiveRealityBlocks - 2);  // 3연속부터 시작
            _currentRealityProbability = Mathf.Max(0f, _currentRealityProbability - penalty);
        }
    }

    private void UpdateProbabilityAfterDreamBlock()
    {
        _consecutiveDreamBlocks++;
        _consecutiveRealityBlocks = 0;  // 현실 블록 연속 카운트 리셋
        
        // 꿈 블록 등장 시 현실 블록 확률 증가
        float bonus = DREAM_BONUS * _consecutiveDreamBlocks;
        _currentRealityProbability = Mathf.Min(1f, BASE_REALITY_PROBABILITY + bonus);
    }

    private void ResetProbability()
    {
        _currentRealityProbability = BASE_REALITY_PROBABILITY;
        _consecutiveRealityBlocks = 0;
        _consecutiveDreamBlocks = 0;
        
        // Index 리셋
        _lastRealityBlockIndex = INVALID_INDEX;
        _lastDreamBlockIndex = INVALID_INDEX;
        
        // 회전 리셋
        _nextBlockRotation = 0f;
        
        Debug.Log("Block probability system reset to default values");
    }

    private void UpdateNextBlockImage()
    {
        if (_nextBlockImage == null || _nextBlockPrefab == null)
            return;

        SpriteRenderer spriteRenderer = _nextBlockPrefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning("Next block prefab does not have a valid sprite renderer.");
            return;
        }

        _nextBlockImage.sprite = spriteRenderer.sprite;
        _nextBlockImage.SetNativeSize();
        
        // UI 이미지에 회전 적용
        _nextBlockImage.transform.rotation = Quaternion.Euler(0f, 0f, _nextBlockRotation);
    }

    private void UpdateMaxHeight()
    {
        if (_targetBlocks.Count == 0)
            return;

        float currentMaxY = FindMaxYPosition();
        
        if (currentMaxY > _maxYBlockPosition)
        {
            _maxYBlockPosition = currentMaxY;
            NotifyMaxHeightChanged();
        }
    }

    private float FindMaxYPosition()
    {
        float maxY = float.MinValue;
        
        foreach (Transform block in _targetBlocks)
        {
            if (block != null && block.position.y > maxY)
            {
                maxY = block.position.y;
            }
        }
        
        return maxY;
    }

    private void NotifyMaxHeightChanged()
    {
        OnMaxHeightChanged?.Invoke(_maxYBlockPosition);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMaxHeight(_maxYBlockPosition);
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null. Cannot update max height.");
        }
    }

    private int GetRandomIndexExcludingLast(int maxIndex, int lastIndex)
    {
        if (maxIndex <= 1) return 0; // 하나 이하의 인덱스는 무조건 0

        int newIndex;
        do
        {
            newIndex = Random.Range(0, maxIndex);
        } while (newIndex == lastIndex);

        return newIndex;
    }

    #endregion
}
