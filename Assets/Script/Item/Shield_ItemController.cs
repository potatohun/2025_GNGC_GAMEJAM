using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield_ItemController : ItemController
{
    protected override void UseItem()
    {
        base.UseItem();
        
        GameManager.Instance.AddShieldItem(1);
    }
}
