using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[System.Serializable]
public class ItemData {
    public bool acquisition;
    public string key;
    public float getHeight;
    public float spawnHeight;
    public string title;
    [TextArea(2, 5)]    public string text;
    public Sprite icon;
    public GameObject itemPrefab;
}

[System.Serializable]
public class SpawnTargetItemData {
    public float spawnHeight;
    public GameObject itemPrefab;
}

public class ItemManager : MonoBehaviour
{
    private const float _newItemPanelWidth = 400f;
    private const float _newItemShowPosX = 25f;
    private const float _newItemShowTime = 10f;
    public static ItemManager Instance { get; private set; }
    [Header("UI")]
    [SerializeField] private RectTransform _newItemPanel;
    [SerializeField] private Image _newItemIcon;
    [SerializeField] private TMP_Text _newItemTitle;
    [SerializeField] private TMP_Text _newItemText;

    [Header("Item Settings")]
    [SerializeField] private List<ItemData> _itemDataList;  // 아이템 프리팹 배열
    [SerializeField] private float _spawnWidthMin = -30f;  // 최소 생성 폭
    [SerializeField] private float _spawnWidthMax = 30f;   // 최대 생성 폭

    [Header("Spawn Target Items")]
    [SerializeField] private List<SpawnTargetItemData> _spawnTargetItemDataList;

    [Header("Spawned Items")]
    [SerializeField] private List<GameObject> _spawnedItems = new List<GameObject>();

    private Tweener _newItemPanelTweener;

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

    private void Start() {
        _newItemPanel.gameObject.SetActive(false);
        _newItemPanel.anchoredPosition = new Vector2(-(_newItemShowPosX + _newItemPanelWidth), 0);

        // 아이템 데이터 로드
        LoadItemGetData();
    }

    public void LoadItemGetData() {
        for(int i = 0; i < _itemDataList.Count; i++) {
            _itemDataList[i].acquisition = DataManager.Instance.GetGetItem(_itemDataList[i].key) == 1;

            if(_itemDataList[i].acquisition) {
                // 생성 타겟 아이템에 추가
                _spawnTargetItemDataList.Add(new SpawnTargetItemData() {
                    spawnHeight = _itemDataList[i].spawnHeight,
                    itemPrefab = _itemDataList[i].itemPrefab
                });
            }
        }
    }

    public void CheckGetNewItem(Vector3 position) {
        for(int i = 0; i < _itemDataList.Count; i++) {
            if(_itemDataList[i].acquisition == false && position.y >= _itemDataList[i].getHeight) {
                // 신규 아이템 획득
                GetNewItem(i);
                this.SpawnItem(position, i);
                return;
            }
        }

        // 신규 아이템 획득 없을 경우 랜덤 아이템 생성
        this.SpawnItemOnLevelUp(position);
    }

    public float GetNextItemHeight() {
        for(int i = 0; i < _itemDataList.Count; i++) {
            if(_itemDataList[i].acquisition == false) {
                return _itemDataList[i].getHeight;
            }
        }
        return -1f;
    }

    public void GetNewItem(int index) {
        if(_newItemPanelTweener != null) {
            _newItemPanelTweener.Kill();
            _newItemPanelTweener = null;
        }

        // 아이템 획득 데이터 로드
        _itemDataList[index].acquisition = true;
        DataManager.Instance.SetGetItem(_itemDataList[index].key, 1);

        // 아이템 생성 테스트
        _newItemPanel.gameObject.SetActive(true);
        _newItemPanelTweener = _newItemPanel.DOAnchorPosX(_newItemShowPosX, 0.5f).SetEase(Ease.OutSine).OnComplete(() => {
            _newItemPanelTweener = null;
            Invoke("HideNewItem", _newItemShowTime);
        });

        // 아이템 데이터 관리리
        _newItemIcon.sprite = _itemDataList[index].icon;
        _newItemTitle.text = _itemDataList[index].title;
        _newItemText.text = _itemDataList[index].text;

        // 생성 타겟 아이템에 추가
        _spawnTargetItemDataList.Add(new SpawnTargetItemData() {
            spawnHeight = _itemDataList[index].spawnHeight,
            itemPrefab = _itemDataList[index].itemPrefab
        });
    }

    public void HideNewItem() {
        _newItemPanelTweener = _newItemPanel.DOAnchorPosX(-(_newItemShowPosX + _newItemPanelWidth), 0.5f).SetEase(Ease.InSine).OnComplete(() => {
            _newItemPanelTweener = null;
            _newItemPanel.gameObject.SetActive(false);
        });
    }

    public void SpawnItemOnLevelUp(Vector3 highestBlockPosition)
    {
        if (_spawnTargetItemDataList == null || _spawnTargetItemDataList.Count == 0)
        {
            Debug.LogWarning("No item prefabs assigned to ItemManager!");
            return;
        }

        int itemIndex = Random.Range(0, _spawnTargetItemDataList.Count);
        
        SpawnTargetItemData spawnTargetItemData = _spawnTargetItemDataList[itemIndex];

        if (spawnTargetItemData.itemPrefab == null)
        {
            Debug.Log($"Item prefab at index {itemIndex} is null!");
            return;
        }

        float baseY = highestBlockPosition.y;
        float baseX = highestBlockPosition.x;

        // 랜덤 위치 계산
        float positionY = baseY + spawnTargetItemData.spawnHeight;
        float positionX = baseX > 0 ? _spawnWidthMin : _spawnWidthMax;
        positionX += Random.Range(-5, 5);
        
        Vector3 spawnPosition = new Vector3(positionX, positionY, 0f);

        // 아이템 생성
        GameObject spawnedItem = Instantiate(spawnTargetItemData.itemPrefab, spawnPosition, Quaternion.identity);
        
        if (spawnedItem != null)
        {
            _spawnedItems.Add(spawnedItem);
            Debug.Log($"Item spawned at position: {spawnPosition}, Item: {spawnedItem.name}");
        }
        else
        {
            Debug.LogError("Failed to instantiate item!");
        }
    }

    public void SpawnItem(Vector3 position, int itemIndex)
    {
        if (_itemDataList == null || _itemDataList.Count == 0)
        {
            Debug.LogWarning("No item prefabs assigned to ItemManager!");
            return;
        }

        ItemData itemData = _itemDataList[itemIndex];

        if (itemData.itemPrefab == null)
        {
            Debug.LogError($"Item prefab at index {itemIndex} is null!");
            return;
        }

        float baseY = position.y;
        float baseX = position.x;

        // 랜덤 위치 계산
        float positionY = baseY + itemData.spawnHeight;
        float positionX = baseX > 0 ? _spawnWidthMin : _spawnWidthMax;
        positionX += Random.Range(-5, 5);
        
        Vector3 spawnPosition = new Vector3(positionX, positionY, 0f);

        // 아이템 생성
        GameObject spawnedItem = Instantiate(itemData.itemPrefab, spawnPosition, Quaternion.identity);
        
        if (spawnedItem != null)
        {
            _spawnedItems.Add(spawnedItem);
            Debug.Log($"Item spawned at position: {spawnPosition}, Item: {spawnedItem.name}");
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
