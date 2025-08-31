using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal_ItemController : ItemController
{
    protected override void UseItem()
    {
        base.UseItem();
        GameManager.Instance.HealAll();
    }
}
