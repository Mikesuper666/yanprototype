using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;


    public class ItemWindowDisplay : MonoBehaviour
    {
        public Inventory inventory;
        public ItemWindow itemWindow;     
        public ItemOptionWindow optionWindow;
        [HideInInspector]
        public ItemSlot currentSelectedSlot;
        [HideInInspector]
        public int amount;
     
        public void OnEnable()
        {
            if (inventory == null)
                inventory = GetComponentInParent<Inventory>();
          
            if (inventory && itemWindow)
                itemWindow.CreateEquipmentWindow(inventory.items, OnSubmit, OnSelectSlot);
        }

        public void OnSubmit(ItemSlot slot)
        {
            currentSelectedSlot = slot;
            if (slot.item)
            {
                var rect = slot.GetComponent<RectTransform>();
                optionWindow.transform.position = rect.position;
                optionWindow.gameObject.SetActive(true);
                optionWindow.EnableOptions(slot);
                currentSelectedSlot = slot;
            }
        }

        public void OnSelectSlot(ItemSlot slot)
        {
            currentSelectedSlot = slot;          
        }

        public void DropItem()
        {            
            if (amount > 0)
            {
                inventory.OnDropItem(currentSelectedSlot.item, amount);
                if (currentSelectedSlot.item.amount <= 0)
                {                    
                    if (itemWindow.slots.Contains(currentSelectedSlot))
                        itemWindow.slots.Remove(currentSelectedSlot);
                    Destroy(currentSelectedSlot.gameObject);
                    if (itemWindow.slots.Count > 0)
                        SetSelectable(itemWindow.slots[0].gameObject);
                }
            }
        }

        public void LeaveItem()
        {
            if (amount > 0)
            {
                inventory.OnLeaveItem(currentSelectedSlot.item, amount);
                if (currentSelectedSlot.item.amount <= 0)
                {
                    if (itemWindow.slots.Contains(currentSelectedSlot))
                        itemWindow.slots.Remove(currentSelectedSlot);
                    Destroy(currentSelectedSlot.gameObject);
                    if (itemWindow.slots.Count > 0)
                        SetSelectable(itemWindow.slots[0].gameObject);
                }
            }
        }

        public void UseItem()
        {
            currentSelectedSlot.item.amount--;
            inventory.OnUseItem(currentSelectedSlot.item);
            if (currentSelectedSlot.item.amount <= 0)
            {
                if (itemWindow.slots.Contains(currentSelectedSlot))
                    itemWindow.slots.Remove(currentSelectedSlot);
                Destroy(currentSelectedSlot.gameObject);
                if (itemWindow.slots.Count > 0)
                    SetSelectable(itemWindow.slots[0].gameObject);
            }
        }

        public void SetOldSelectable()
        {
            try
            {
                if (currentSelectedSlot != null)
                    SetSelectable(currentSelectedSlot.gameObject);
                else if (itemWindow.slots.Count > 0 && itemWindow.slots[0] != null)
                {
                    SetSelectable(itemWindow.slots[0].gameObject);
                }
            }
            catch
            {

            }
        }

        void SetSelectable(GameObject target)
        {
            try
            {
                var pointer = new PointerEventData(EventSystem.current);
                ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerExitHandler);
                EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
                ExecuteEvents.Execute(target, pointer, ExecuteEvents.selectHandler);
            }
            catch { }

        }

    }


