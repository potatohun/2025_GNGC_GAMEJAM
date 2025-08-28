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

        // 랜덤 아이템 선택
        int randomItemIndex = Random.Range(0, _itemPrefabs.Count);
        if(randomItemIndex == 0) {
            // Do Nothing
            return;
        }
        ItemHeightSetting itemHeightSetting = _itemPrefabs[randomItemIndex];

        if (itemHeightSetting.itemPrefab == null)
        {
            Debug.LogError($"Item prefab at index {randomItemIndex} is null!");
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
            Debug.Log($"Item spawned at position: {spawnPosition}, Item: {itemHeightSetting.itemPrefab.name}");
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
