using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("Item Settings")]
    [SerializeField] private GameObject[] _itemPrefabs;  // 아이템 프리팹 배열
    [SerializeField] private float _spawnHeightMin = 10f;
    [SerializeField] private float _spawnHeightMax = 15f;
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
    public void SpawnItemOnLevelUp()
    {
        if (_itemPrefabs == null || _itemPrefabs.Length == 0)
        {
            Debug.LogWarning("No item prefabs assigned to ItemManager!");
            return;
        }

        // 랜덤 아이템 선택
        int randomItemIndex = Random.Range(0, _itemPrefabs.Length);
        if(randomItemIndex == 0) {
            // Do Nothing
            return;
        }
        GameObject itemPrefab = _itemPrefabs[randomItemIndex];

        if (itemPrefab == null)
        {
            Debug.LogError($"Item prefab at index {randomItemIndex} is null!");
            return;
        }

        float baseY = GameManager.Instance.GetMaxHeight();

        // 랜덤 위치 계산
        float randomY = Random.Range(baseY + _spawnHeightMin, baseY + _spawnHeightMax);
        float randomX = Random.Range(_spawnWidthMin, _spawnWidthMax);
        Vector3 spawnPosition = new Vector3(randomX, randomY, 0f);

        // 아이템 생성
        GameObject spawnedItem = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        
        if (spawnedItem != null)
        {
            _spawnedItems.Add(spawnedItem);
            Debug.Log($"Item spawned at position: {spawnPosition}, Item: {itemPrefab.name}");
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
