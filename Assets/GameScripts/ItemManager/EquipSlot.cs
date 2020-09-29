using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;


    public class EquipSlot : ItemSlot
    {
        [Header("--- Item Type ---")]
        public List<ItemType> itemType;
        public bool clickToOpen = true;
        public bool autoDeselect = true;

        public override void AddItem(Item item)
        {
            if (item) item.isInEquipArea = true;
            base.AddItem(item);
            onAddItem.Invoke(item);
        }

        public override void RemoveItem()
        {
            onRemoveItem.Invoke(item);
            if (item != null) item.isInEquipArea = false;
            base.RemoveItem();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            if (autoDeselect)
                base.OnDeselect(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (autoDeselect)
                base.OnPointerExit(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (clickToOpen)
                base.OnPointerClick(eventData);
        }
    }
