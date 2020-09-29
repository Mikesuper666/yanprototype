using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


    public class ItemOptionWindow : MonoBehaviour
    {
        public Button useItemButton;
        public List<ItemType> itemsCanBeUsed = new List<ItemType>() { ItemType.Consumable };

        public virtual void EnableOptions(ItemSlot slot)
        {
            if (slot ==null || slot.item==null) return;
            useItemButton.interactable = itemsCanBeUsed.Contains(slot.item.type);
        }        
    }


