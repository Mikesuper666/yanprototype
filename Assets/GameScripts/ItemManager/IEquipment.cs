using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEquipment
{
    void OnEquip(Item item);
    void OnUnequip(Item item);
}
