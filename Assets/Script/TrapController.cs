using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum TrapType {
    Reality,
    Dream,
    All
}

public class TrapController : MonoBehaviour
{
    [SerializeField] private TrapType _trapType;

    void OnTriggerStay2D(Collider2D collision) {
        // TrapType에 맞는 태그를 가진 오브젝트와만 동작
        bool shouldTrigger = false;
        
        switch (_trapType) {
            case TrapType.Reality:
                shouldTrigger = collision.gameObject.CompareTag("RealityBlock");
                break;
            case TrapType.Dream:
                shouldTrigger = collision.gameObject.CompareTag("DreamBlock");
                break;
            case TrapType.All:
                shouldTrigger = collision.gameObject.CompareTag("RealityBlock") || collision.gameObject.CompareTag("DreamBlock");
                break;
        }
        
        if (shouldTrigger) {
            BlockController blockController = collision.gameObject.GetComponent<BlockController>();
            if(blockController == null)
                return;

            if(_trapType == TrapType.Dream) {
                DreamBlockController dreamBlockController = collision.gameObject.GetComponent<DreamBlockController>();
                if(dreamBlockController != null) {
                    if(dreamBlockController.GetFloating() == false) {
                        return;
                    }
                }
            }

            blockController.TriggerTrap();
        }
    }
}
