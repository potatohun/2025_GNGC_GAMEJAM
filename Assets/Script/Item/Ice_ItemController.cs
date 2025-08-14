using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ice_ItemController : ItemController
{
    // Start is called before the first frame update
    protected override void UseItem()
    {
        base.UseItem();
        BlockManager.Instance.FixAllBlocks();
        EffectManager.Instance.PlayEffect(EffectType.IceItem);
    }
}
