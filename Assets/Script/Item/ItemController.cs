using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    private bool _isUsed = false;
    private void OnTriggerStay2D(Collider2D other)
    {
        if(_isUsed)
            return;

        // 현실 블록과 충돌했는지 확인
        if (other.CompareTag("RealityBlock"))
        {
            BlockController blockController = other.GetComponent<BlockController>();
            if (blockController != null && blockController.GetIsFalling() == false)
            {
                _isUsed = true;
                UseItem();
            }
        }
    }

    /// <summary>
    /// 아이템을 사용합니다. 자식 클래스에서 오버라이드하여 구현합니다.
    /// </summary>
    protected virtual void UseItem()
    {
        Debug.Log($"Item used: {gameObject.name}");
        
        // 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("ItemCollect");
        }

        // 아이템 제거
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.RemoveItem(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
