using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket_ItemController : ItemController
{
    [SerializeField] private GameObject _object_rocket;
    // Start is called before the first frame update
    protected override void UseItem()
    {
        base.UseItem();

        // 블럭 고정
        BlockManager.Instance.FixAllBlocks();

        // 로켓 오브젝트 활성화 및 효과 실행행
        Instantiate(_object_rocket, MapManager.Instance.GetLookAtTarget().position, Quaternion.identity);
        MapManager.Instance.RocketItemEffect();
        EffectManager.Instance.PlayEffect(EffectType.RocketItem);
    }
}
