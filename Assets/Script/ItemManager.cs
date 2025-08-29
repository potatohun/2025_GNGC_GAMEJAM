using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

[System.Serializable]
public class ItemHeightSetting {
    public float height;
    public GameObject itemPrefab;
}

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("Item Settings")]
    [SerializeField] private List<ItemHeightSetting> _itemPrefabs;  // 아이템 프리팹 배열
    [SerializeField] private float _spawnWidthMin = -30f;  // 최소 생성 폭
    [SerializeField] private float _spawnWidthMax = 30f;   // 최대 생성 폭

    [Header("Spawned Items")]
    [SerializeField] private List<GameObject> _spawnedItems = new List<GameObject>();
    
    // 아이템 생성 순서 추적
    private int _itemSpawnCount = 0;
    private readonly int[] _initialItemOrder = { 2, 3, 4, 5 }; // 하트, 얼음, 쉴드, 로켓 순서

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple ItemManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }
    public void SpawnItemOnLevelUp(Vector3 highestBlockPosition)
    {
        if (_itemPrefabs == null || _itemPrefabs.Count == 0)
        {
            Debug.LogWarning("No item prefabs assigned to ItemManager!");
            return;
        }

        int itemIndex;
        
        // 처음 4번은 고정 순서로 생성
        if (_itemSpawnCount < 4)
        {
            itemIndex = _initialItemOrder[_itemSpawnCount];
            Debug.Log($"Fixed item spawn #{_itemSpawnCount + 1}: Index {itemIndex}");
        }
        else
        {
            // 5번째 이후부터는 랜덤 생성 (0번 제외)
            itemIndex = Random.Range(1, _itemPrefabs.Count);
            Debug.Log($"Random item spawn #{_itemSpawnCount + 1}: Index {itemIndex}");
        }
        
        if(itemIndex == 0) {
            // Do Nothing
            return;
        }
        
        ItemHeightSetting itemHeightSetting = _itemPrefabs[itemIndex];

        if (itemHeightSetting.itemPrefab == null)
        {
            Debug.LogError($"Item prefab at index {itemIndex} is null!");
            return;
        }

        float baseY = highestBlockPosition.y;
        float baseX = highestBlockPosition.x;

        // 랜덤 위치 계산
        float positionY = baseY + itemHeightSetting.height;
        float positionX = baseX > 0 ? _spawnWidthMin : _spawnWidthMax;
        positionX += Random.Range(-5, 5);
        
        Vector3 spawnPosition = new Vector3(positionX, positionY, 0f);

        // 아이템 생성
        GameObject spawnedItem = Instantiate(itemHeightSetting.itemPrefab, spawnPosition, Quaternion.identity);
        
        if (spawnedItem != null)
        {
            _spawnedItems.Add(spawnedItem);
            _itemSpawnCount++; // 아이템 생성 카운트 증가
            Debug.Log($"Item spawned at position: {spawnPosition}, Item: {itemHeightSetting.itemPrefab.name}, Spawn Count: {_itemSpawnCount}");
        }
        else
        {
            Debug.LogError("Failed to instantiate item!");
        }
    }
    
    public void RemoveItem(GameObject item)
    {
        if (_spawnedItems.Contains(item))
        {
            _spawnedItems.Remove(item);
            Destroy(item);
        }
    }
}
